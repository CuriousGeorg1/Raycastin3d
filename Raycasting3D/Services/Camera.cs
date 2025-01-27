using System;

namespace Raycasting3D.Services;

public class Camera
{
    public Location2D Cameralocation { get; private set; }
    public double Direction { get; private set; }

    public Camera(double initialX = 10, double initialY = 7, double initialDirection = 0)   
    {
        Cameralocation = new Location2D {X = initialX, Y = initialY};
        Direction = initialDirection;
    }
    
    public void Move(double deltaX, double deltaY)
    {
        Cameralocation = new Location2D
        {
            X = Cameralocation.X + deltaX, 
            Y = Cameralocation.Y + deltaY
        };
    }
    
    public void Rotate(double deltaDirection)
    {
        Direction += deltaDirection;
    }
    
    public Location2D GetLocation() => Cameralocation;
    
    public double GetDirection() => Direction;
}


