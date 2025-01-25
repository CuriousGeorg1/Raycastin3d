namespace Raycasting;

public partial class MainPage : ContentPage
{
	private Location2D cameraLocation;


	public MainPage()
	{
		InitializeComponent();
		cameraLocation = new Location2D { X = 10, Y = 7 };
	}

	private void OnRenderRaycast(object sender, EventArgs e)
	{
		var columns = Program.CastRays(cameraLocation, 1024);
		var bmprender = new BitmapRenderer(768, 1024, Program.MaxCameraRange);
		var bitmap = bmprender.RenderBitmap(columns);

		var image = new Image { Source = ImageSource.FromStream(() => bitmap.AsStream()) };
		RenderImage.Source = image;
	}

	private void OnMoveCamera(object sender, EventArgs e)
	{
		var button = (Button)sender;
		var direction = button.Text switch
		{
			"Up" => 0,
			"Down" => 180,
			"Left" => 270,
			"Right" => 90,
			_ => 0
		};

		cameraLocation = MoveCamera(cameraLocation, direction);
	}
}

class Program
    {

        public static int MaxCameraRange = 20;
        public static string[] World;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            World = new[]
            {
                "########################################",
                "#                                      #",
                "#                      ###             #",
                "#                                      #",
                "#                      ###             #",
                "#                      ###             #",
                "#                                      #",
                "#                                      #",
                "#                                      #",
                "#                                      #",
                "#                                      #",
                "#                                      #",
                "########################################",
            };

            var cameraLocation = new Location2D {X = 10, Y = 7};
            var columns = CastRays(cameraLocation, 1024);

            var bmprender = new BitmapRenderer(768, 1024, MaxCameraRange);
            var bitmap = bmprender.RenderBitmap(columns);

            using var ms = new MemoryStream();
            bitmap.SaveAsBmp(ms);
            File.WriteAllBytes("sample.bmp", ms.ToArray());
            Console.WriteLine("Done");
        }

        public static Ray.SamplePoint[] CastRays(Location2D origin, int renderWidth, int directionInDegrees = 0)
        {
            var result = new Ray.SamplePoint[renderWidth];

            for (var column = 0; column < renderWidth; column++)
            {
                var x = (double)column / renderWidth - 0.5;

                var startPoint = new Ray.SamplePoint(origin);
                var castDirection = ComputeDirection(directionInDegrees, x);
                var ray = Ray(startPoint, castDirection);

                result[column] = ray[^1];
            }

            return result;
        }

        private static CastDirection ComputeDirection(double directionDegrees, double angle)
        {
            var radians = Math.PI / 180 * directionDegrees;
            var directionInDegrees = radians + angle;
            return new CastDirection(directionInDegrees);
        }

        private static Ray Ray(Ray.SamplePoint origin, CastDirection castDirection)
        {
        var rayPath = new Ray();
        var currentStep = origin;

        while (true)
        {
            rayPath.Add(currentStep);

            var stepX = NextStepOnTheLine(
                castDirection.Sin,
                castDirection.Cos,
                currentStep.Location.X,
                currentStep.Location.Y
            );

            var stepY = NextStepOnTheLine(
                castDirection.Cos,
                castDirection.Sin,
                currentStep.Location.Y,
                currentStep.Location.X,
                true
            );

            var shortestStep = stepX.Length < stepY.Length 
            ? Inspect(stepX,1,0,currentStep.DistanceTraveled, castDirection) :
            Inspect(stepY,0,1,currentStep.DistanceTraveled, castDirection);

            if (shortestStep.Surface.HasNoHeight)
            {
                currentStep = shortestStep;
                continue;
            }
            if (shortestStep.DistanceTraveled > MaxCameraRange)
            {
                return rayPath;
            }
            rayPath.Add(shortestStep);
            return rayPath;
        }

    }

        private static Ray.SamplePoint NextStepOnTheLine (double rise, double run, double firstValue, double secondValue, bool intverted = false)
{
    var steppedFirst = run > 0 ? Math.Floor(firstValue +1) - firstValue : Math.Ceiling(firstValue - 1) - firstValue;
    var steppedSecond = steppedFirst * (rise / run);

    var lenth = steppedFirst * steppedFirst + steppedSecond * steppedSecond;

    var location2D = new Location2D
    {
        X = firstValue + steppedFirst,
        Y = secondValue + steppedSecond
    };

    location2D = intverted ? location2D.FlipXY() : location2D;

    return new Ray.SamplePoint(location2D, lenth);
}

private static Ray.SamplePoint Inspect(
    Ray.SamplePoint step,
    int shiftX,
    int shiftY,
    double distanceTraveled,
    CastDirection castDirection)
    {
        var dx = castDirection.Cos < 0 ? shiftX : 0;
        var dy = castDirection.Sin < 0 ? shiftY : 0;

        step.Surface = DetectSurface(step.Location.X - dx, step.Location.Y - dy);
        step.DistanceTraveled = distanceTraveled + Math.Sqrt(step.Length);
        return step;
    }

        private static Surface DetectSurface(double xDouble, double yDouble)
    {
        var x = (int)Math.Floor(xDouble);
        var y = (int)Math.Floor(yDouble); 
        return SurfaceAt(x, y);
    }

        private static Surface SurfaceAt(int x, int y)
    {
        var glyph = World[y][x];
        return glyph == '#' 
        ? new Surface { Height = 1}
        : Surface.Nothing;
    }

    }  // class Program

    public class BitmapRenderer
    {
        private readonly int _range;
        public int ImageHeight { get; }
        public int ImageWidth { get; }

        public BitmapRenderer(int imageHeight, int imageWidth, int range)
        {
            ImageHeight = imageHeight;
            ImageWidth = imageWidth;
            _range = range;
        }


        public Image<Rgba32> RenderBitmap(IReadOnlyList<Ray.SamplePoint> columnData)
    {
        var pixels = new Image<Rgba32>(ImageWidth, ImageHeight);

        for (var column = 0; column < columnData.Count; column++)
        {
            var samplePoint = columnData[column];
            var maxPossibleHeight = ImageHeight * samplePoint.Surface.Height;
            var height = maxPossibleHeight / (samplePoint.DistanceTraveled /2.5);

            height = Math.Min(height, ImageHeight);

            var verticalPadding = (int)Math.Floor((ImageHeight - height)/2);

            var texture = SelectTexture(samplePoint);

            for (var y = verticalPadding; y < ImageHeight - verticalPadding; y++)
            {
                pixels[column, y] = texture;
            }
            
        }
        return pixels;
    }

    private Rgba32 SelectTexture(Ray.SamplePoint samplePoint)
    {
        var percentage = (samplePoint.DistanceTraveled / _range) * 100;
            var brightness = 200 - ((200.00 / 100) * percentage);

            return new Rgba32(
                (byte)brightness, 
                (byte)brightness, 
                (byte)brightness);
    }

} // class BitmapRenderer

public struct Location2D
{
     public double X;
    public double Y;
    
    public Location2D FlipXY()
    {
        return new Location2D {X = Y, Y = X};
    }
}

    public readonly struct CastDirection
    {
        public double Sin { get; }
        public double Cos { get; }

        public CastDirection(double angle)
        {
            Sin = Math.Sin(angle);
            Cos = Math.Cos(angle);
        }
    }

public struct Surface
{
    public double Height { get; set; }
    public bool HasNoHeight => Height == 0;
    public static Surface Nothing {get;} = new Surface();
}

    
    public class Ray: List<Ray.SamplePoint>
{



    public struct SamplePoint
    {
        public Location2D Location { get; set; }
        public double Length { get; set; }
        public double DistanceTraveled { get; set; } 

        public Surface Surface { get; set; }

        public SamplePoint(
            Location2D location2D,
            double length = 0,
            double distanceTraveled = 0)
            {
                Location = location2D;
                Length = length;
                DistanceTraveled = distanceTraveled;
                Surface = Surface.Nothing;
            }
        
    }


} // class Ray
