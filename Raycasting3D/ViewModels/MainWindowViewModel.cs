using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using ReactiveUI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Raycasting3D.Services;

namespace Raycasting3D.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly Camera _camera;
        private readonly BitmapRenderer _renderer;
        private Bitmap _renderedImage;

        public MainWindowViewModel()
        {
            _camera = new Camera();
            _renderer = new BitmapRenderer(450, 800, 100);
            RenderScene();

            MoveUpCommand = ReactiveCommand.Create(() => MoveCamera(0, -1));
            MoveDownCommand = ReactiveCommand.Create(() => MoveCamera(0, 1));
            MoveLeftCommand = ReactiveCommand.Create(() => MoveCamera(-1, 0));
            MoveRightCommand = ReactiveCommand.Create(() => MoveCamera(1, 0));
        }

        public Bitmap RenderedImage
        {
            get => _renderedImage;
            private set => this.RaiseAndSetIfChanged(ref _renderedImage, value);
        }

        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand MoveLeftCommand { get; }
        public ICommand MoveRightCommand { get; }

        private void MoveCamera(double deltaX, double deltaY)
        {
            _camera.Move(deltaX, deltaY);
            RenderScene();
        }

        private void RenderScene()
        {
            var columns = RaycastingServices.CastRays(_camera.Cameralocation, 450);
            var bitmap = _renderer.RenderBitmap(columns);

            using var ms = new MemoryStream();
            bitmap.SaveAsBmp(ms);
            RenderedImage = new Bitmap(ms);
        }
    }
}
