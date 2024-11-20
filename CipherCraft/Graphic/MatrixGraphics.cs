using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CipherCraft
{
    public struct Vec3
    {
        public double x, y, z;
    }
    public struct Vec2
    {
        public int x, y;
    }
    public struct Tetrahedron
    {
        public Vec3[] vec3s;
        public void Copy(Tetrahedron t)
        {
            vec3s[0].x = t.vec3s[0].x;
            vec3s[0].y = t.vec3s[0].y;
            vec3s[0].z = t.vec3s[0].z;
            vec3s[1].x = t.vec3s[1].x;
            vec3s[1].y = t.vec3s[1].y;
            vec3s[1].z = t.vec3s[1].z; 
            vec3s[2].x = t.vec3s[2].x;
            vec3s[2].y = t.vec3s[2].y;
            vec3s[2].z = t.vec3s[2].z;
            vec3s[3].x = t.vec3s[3].x;
            vec3s[3].y = t.vec3s[3].y;
            vec3s[3].z = t.vec3s[3].z;
            vec3s[4].x = t.vec3s[4].x;
            vec3s[4].y = t.vec3s[4].y;
            vec3s[4].z = t.vec3s[4].z;
        }
    }
    public class MatrixGraphics
    {
        double[] sinFromDeg = new double[360];
        double[] cosFromDeg = new double[360];

        public MatrixGraphics()
        {
            for (int i = 0; i < sinFromDeg.Length; i++)
            {
                double rad = (2 * Math.PI) * (i / (double)sinFromDeg.Length);
                sinFromDeg[i] = Math.Sin(rad);
                cosFromDeg[i] = Math.Cos(rad);
            }
        }
        double sin(int d) { return sinFromDeg[d]; }
        double cos(int d) { return cosFromDeg[d]; }

        public void rot(int rollDeg, int pitchDeg, int yawDeg, Vec3[] vec3s)
        {
            //a = roll; B = pitch; y = yaw 
            double sinRoll = sin(rollDeg);
            double cosRoll = cos(rollDeg);
            double sinPitch = sin(pitchDeg);
            double cosPitch = cos(pitchDeg);
            double sinYaw = sin(yawDeg);
            double cosYaw = cos(yawDeg);

            double x_ = cosRoll * cosPitch;
            double xx = (cosRoll * sinPitch * sinYaw) - (sinRoll * cosYaw);
            double xxx = (cosRoll * sinPitch * cosYaw) + (sinRoll * sinYaw);
            double xy = sinRoll * cosPitch;
            double xxy = (sinRoll * sinPitch * sinYaw) + (cosRoll * cosYaw);
            double xxxy = (sinRoll * sinPitch * cosYaw) - (cosRoll * sinYaw);
            double xyy = -sinPitch;
            double xxyy = cosPitch * sinYaw;
            double xxxyy = cosPitch * cosYaw;

            for (int i = 0; i < vec3s.Length; i++)
            {
                double x = vec3s[i].x;
                double y = vec3s[i].y;
                double z = vec3s[i].z;

                vec3s[i].x = (x_ * x) + (xx * y) + (xxx * z);
                vec3s[i].y = (xy * x) + (xxy * y) + (xxxy * z);
                vec3s[i].z = (xyy * x) + (xxyy * y) + (xxxyy * z);
            }
        }
        public void trans(double x, double y, double z, Vec3[] vec3s)
        {
            for (int i = 0; i < vec3s.Length; i++)
            {
                vec3s[i].x += x;
                vec3s[i].y += y;
                vec3s[i].z += z;
            }
        }
        public void ConvertToScreen(Vec3[] vec3s, Vec2[] returnToScreen, double DEPTH, int xCenter, int yCenter)
        {
            for (int i = 0; i < vec3s.Length; i++)
            {
                double scaleProjected = DEPTH / (DEPTH + vec3s[i].z);
                returnToScreen[i].x = (int)((vec3s[i].x * scaleProjected) + xCenter);
                returnToScreen[i].y = (int)((vec3s[i].y * scaleProjected) + yCenter);
            }
        }
    }
}
