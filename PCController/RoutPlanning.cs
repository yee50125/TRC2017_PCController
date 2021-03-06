﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCController
{
    class ArmData
    {
        public static double longbase;
        public static double longrate2;
        public static double longrate3;
    }

    class MotorAngle
    {
        public MotorAngle(double motor1angle_in, double motor2angle_in, double motor3angle_in, double motor4angle_in)
        {
            motor1angle = motor1angle_in;
            motor2angle = motor2angle_in;
            motor3angle = motor3angle_in;
            motor4angle = motor4angle_in;
        }

        public double motor1angle, motor2angle, motor3angle, motor4angle;
    }

    class AngleList
    {
        public Angle headAngle;
        public AngleList(decimal one_in, decimal two_in, decimal three_in, decimal four_in)
        {
            headAngle = new Angle(one_in, two_in, three_in, four_in);
        }
    }

    class Angle
    {
        public decimal one, two, three, four;
        public Angle nextangle;
        public Angle(decimal one_in, decimal two_in, decimal three_in, decimal four_in)
        {
            one = one_in;
            two = two_in;
            three = three_in;
            four = four_in;

            nextangle = null;
        }
        public void creatNextNode(decimal one_in, decimal two_in, decimal three_in, decimal four_in)
        {
            nextangle = new Angle(one_in, two_in, three_in, four_in);
        }
    }


    class RoutPlanning
    {
        public static int pointCount;


        private static MotorAngle calcu(MotorAngle origangle, double distance, int pointsnum)
        {
            pointCount = pointsnum;

            double pi = 3.1415926;
            int count = 0;

            double arm1long = ArmData.longbase;
            double arm2long = arm1long * (ArmData.longrate2);
            double arm3long = arm1long * (ArmData.longrate3);


            if (arm1long == 0 || arm2long == 0 || arm3long == 0)
            {
                Program.form.showWarnning(string.Format("arm1long = {0:f3}, arm2long = {1:f3},arm3long = {2:f3}", arm1long, arm2long, arm3long));
                return null;
            }


            double angle1 = (origangle.motor1angle) * pi / 180;
            double angle2 = (origangle.motor2angle) * pi / 180;
            double angle3 = (origangle.motor3angle) * pi / 180;
            double angle4 = (origangle.motor4angle) * pi / 180;

            Program.form.mesPrintln(String.Format("initial angle of 1,3,4:{0:f},{1:f},{2:f}", origangle.motor1angle, origangle.motor3angle, origangle.motor4angle));

            double tmpangle1, tmpangle7, tmpangle8, tmpangle4, tmpangle2, tmpangle9, tmpangle6;
            double tmpline1;//origangle2,arm1,arm2

            double movelong = distance / pointsnum;

            double afterline1 = 0, afterangle1 = 0, afterangle3 = 0, afterangle4 = 0;

            StreamWriter angleFileWriter = new StreamWriter("moveangle.txt");

            /*
                //check if the distance is able to move
                tmpline1=pow(arm1long,2)+pow(arm2long,2)-2*arm1long*arm2long*cos(angle3);
                tmpline1=Math.Sqrt(tmpline1);
                if(distance>(arm1long+arm2long)-tmpline1){
                printf("the distance is too long,the longest distance is %f\n",tmpline1);
                }
                //
            */

            for (count = 0; count < pointsnum; count++)
            {

                tmpline1 = Math.Pow(arm1long, 2) + Math.Pow(arm2long, 2) - 2 * arm1long * arm2long * Math.Cos(angle3);
                tmpline1 = Math.Sqrt(tmpline1);

                if (tmpline1 == 0)
                {
                    Program.form.showWarnning(string.Format("tmpline1 = {0:f3}", tmpline1));
                    angleFileWriter.Close();
                    return null;
                }


                tmpangle8 = (Math.Pow(tmpline1, 2) + Math.Pow(arm1long, 2) - Math.Pow(arm2long, 2)) / (2 * arm1long * tmpline1);
                tmpangle8 = Math.Acos(tmpangle8);

                tmpangle7 = (Math.Pow(tmpline1, 2) + Math.Pow(arm2long, 2) - Math.Pow(arm1long, 2)) / (2 * arm2long * tmpline1);
                tmpangle7 = Math.Acos(tmpangle7);

                tmpangle4 = angle4 + tmpangle7;

                afterline1 = Math.Pow(tmpline1, 2) + Math.Pow(movelong, 2) - 2 * tmpline1 * movelong * Math.Cos(tmpangle4);
                afterline1 = Math.Sqrt(afterline1);

                afterangle3 = (Math.Pow(arm2long, 2) + Math.Pow(arm1long, 2) - Math.Pow(afterline1, 2)) / (2 * arm1long * arm2long);
                afterangle3 = Math.Acos(afterangle3);


                if (afterline1 == 0)
                {
                    Program.form.showWarnning(string.Format("afterline1 = {0:f3}", afterline1));
                    angleFileWriter.Close();
                    return null;
                }

                tmpangle9 = (Math.Pow(afterline1, 2) + Math.Pow(arm1long, 2) - Math.Pow(arm2long, 2)) / (2 * arm1long * afterline1);
                tmpangle9 = Math.Acos(tmpangle9);

                tmpangle6 = (Math.Pow(tmpline1, 2) + Math.Pow(afterline1, 2) - Math.Pow(movelong, 2)) / (2 * tmpline1 * afterline1);
                if(tmpangle6>1 && tmpangle6 < 1.0001)
                {
                    tmpangle6 = 1;
                }


                tmpangle6 = Math.Acos(tmpangle6);

                afterangle1 = angle1 - (tmpangle8 - tmpangle9 - tmpangle6);//

                afterangle4 = pi - (pi - tmpangle4 - tmpangle6) - (pi - afterangle3 - tmpangle9);

                //Program.form.mesPrintln(String.Format("angle1:{0:f} ,angle3:{1:f} ,angle4:{2:f}", afterangle1 * 180 / pi, afterangle3 * 180 / pi, afterangle4 * 180 / pi));


                if (afterangle1 == double.NaN || afterangle3 == double.NaN)
                {
                    Program.form.showWarnning(string.Format("afterangle1 = {0:f3},afterangle3 = {1:f3}", afterangle1, afterangle3));
                    angleFileWriter.Close();
                    return null;
                }

                angleFileWriter.WriteLine("{0:f12},{1:f12}", afterangle1 * 180 / pi, afterangle3 * 180 / pi);

                angle1 = afterangle1;
                angle3 = afterangle3;
                angle4 = afterangle4;
            }

            angleFileWriter.Close();

            MotorAngle afterangle = new MotorAngle(afterangle1, angle2, afterangle3, afterangle4);


            return afterangle;
        }


        public static void Initialize(double[,] coordinate, double distance, double ratio)
        {

            /*
            the first layer(highter one) are 0~3(left to right);second layer(lower one) are 4~7(left to right)
            */
            //int[,] coordinate = new int[10, 4];
            int i;
            double[,] measureangle = new double[10, 4];
            double[,] realcd = new double[5, 2];
            double[] centerx = new double[4];
            double[] centery = new double[4];
            double avcx, avcy;
            double[,] reference = new double[5, 2];
            double armlong1 = ArmData.longbase;
            double armlong2 = armlong1 * ArmData.longrate2;
            double armlong3 = armlong1 * ArmData.longrate3;
            const double pi = 3.1415926;


            //calculate the real(x,y)coordinate(realcd) of each platform
            for (i = 0; i < 5; i++)
            {
                realcd[i, 0] = (armlong1 * Math.Cos(measureangle[i, 0])) + (armlong2 * Math.Cos(measureangle[i, 0] + measureangle[i, 2])) + (armlong3 * Math.Cos(measureangle[i, 0] + measureangle[i, 2] - measureangle[i, 3]));
                realcd[i, 1] = (armlong1 * Math.Sin(measureangle[i, 0])) + (armlong2 * Math.Sin(measureangle[i, 0] + measureangle[i, 2])) + (armlong3 * Math.Sin(measureangle[i, 0] + measureangle[i, 2] - measureangle[i, 3])); ;
            }
            //
            //for test,because no measureangle
            for (i = 0; i < 10; i++)
            {
                measureangle[i, 1] = 0;
            }
            double topanglez = 0, loweranglez = 0;
            realcd[0, 0] = 3.0 * (ratio / 5);
            realcd[0, 1] = 4.0* (ratio / 5);
            realcd[1, 0] = 4.0 * (ratio / 5);
            realcd[1, 1] = 3.0 * (ratio / 5);
            realcd[2, 0] = 5.0 * (ratio / 5);
            realcd[2, 1] = 0.0 * (ratio / 5);
            realcd[3, 0] = 4.0 * (ratio / 5);
            realcd[3, 1] = (-3) * (ratio / 5);
            realcd[4, 0] = 3.0 * (ratio / 5);
            realcd[4, 1] = (-4) * (ratio / 5);
            //need to deleted

            //calculate the center of cycle
            double cx, cy, dx, dy, l, h, tdx, tdy;

            for (i = 0; i < 4; i++)
            {

                cx = (realcd[i, 0] + realcd[i + 1, 0]) / 2;
                cy = (realcd[i, 1] + realcd[i + 1, 1]) / 2;
                if (realcd[i + 1, 1] - realcd[i, 1] < 0)
                {
                    dx = realcd[i + 1, 0] - realcd[i, 0];
                    dy = realcd[i + 1, 1] - realcd[i, 1];
                }
                else
                {
                    dx = realcd[i, 0] - realcd[i + 1, 0];
                    dy = realcd[i, 1] - realcd[i + 1, 1];
                }
                l = Math.Sqrt(dx * dx + dy * dy);
                h = Math.Sqrt(ratio * ratio - (l / 2) * (l / 2));
                tdx = (-1) * h * dy / l;
                tdy = h * dx / l;
                centerx[i] = cx - tdx;
                centery[i] = cy - tdy;
            }
            avcx = (centerx[0] + centerx[1] + centerx[2] + centerx[3]) / 4;
            avcy = (centery[0] + centery[1] + centery[2] + centery[3]) / 4;
            Program.form.mesPrintln(String.Format("中心位置 x:{0:f} y: {1:f}", avcx, avcy));
            //calculate center

            double tmplong1 = 0;//original pointer to forth modor
            double tmplong2 = 0;//avcenter to reference
            double tmpangle1 = 0;//tmplong and y axis;
            double tmpangle2 = 0;//tmplong and (avcenter to reference)


            for (i = 0; i < 5; i++)
            {
                reference[i, 0] = avcx + (realcd[i, 0] - avcx) * ((ratio - distance - armlong3) / ratio);
                reference[i, 1] = avcy + (realcd[i, 1] - avcy) * ((ratio - distance - armlong3) / ratio);
                //printf("\n%f %f %f\n",reference[i][0],reference[i][1],(reference[i][0]-avcx)*(reference[i][0]-avcx)+(reference[i][1]-avcy)*(reference[i][1]-avcy));
                Program.form.mesPrintln(string.Format("各平台直線進入點  x:{0:f} y:{1:f}", reference[i, 0], reference[i, 1]));
            }

            for (i = 0; i < 5; i++)
            {
                tmplong1 = Math.Sqrt(reference[i, 0] * reference[i, 0] + reference[i, 1] * reference[i, 1]);
                tmpangle1 = (reference[i, 1] * 1) / (tmplong1);
                tmpangle1 = (Math.Acos(tmpangle1)) * 180 / pi;
                coordinate[i, 0] = (tmplong1 * tmplong1 + armlong1 * armlong1 - armlong2 * armlong2) / (2 * tmplong1 * armlong1);
                coordinate[i, 0] = (Math.Acos(coordinate[i, 0])) * 180 / pi;
                coordinate[i, 0] = tmpangle1 + coordinate[i, 0];
                coordinate[i, 1] = measureangle[i, 1];
                coordinate[i, 2] = (armlong1 * armlong1 + armlong2 * armlong2 - tmplong1 * tmplong1) / (2 * armlong1 * armlong2);
                coordinate[i, 2] = (Math.Acos(coordinate[i, 2])) * 180 / pi;
                coordinate[i, 3] = (armlong2 * armlong2 + tmplong1 * tmplong1 - armlong1 * armlong1) / (2 * tmplong1 * armlong2);
                coordinate[i, 3] = (Math.Acos(coordinate[i, 3])) * 180 / pi;
                tmplong2 = Math.Sqrt((reference[i, 0] - avcx) * (reference[i, 0] - avcx) + (reference[i, 1] - avcy) * (reference[i, 1] - avcy));
                tmpangle2 = (reference[i, 0] * (reference[i, 0] - avcx) + reference[i, 1] * (reference[i, 1] - avcy)) / (tmplong2 * tmplong1);
                tmpangle2 = (Math.Acos(tmpangle2)) * 180 / pi;
                coordinate[i, 3] = 180 - tmpangle2 - coordinate[i, 3];
                coordinate[i+5, 0] = coordinate[i, 0];
                coordinate[i+5, 1] = coordinate[i, 1];
                coordinate[i + 5, 2] = coordinate[i, 2];
                coordinate[i + 5, 3] = coordinate[i, 3];
                Program.form.mesPrintln(string.Format("各平台直線進入初始四軸角度 1axis:{0:f} 2axis:{1:f} 3axis:{2:f} 4axis:{3:f} \n", coordinate[i, 0], coordinate[i, 1], coordinate[i, 2], coordinate[i, 3]));
            }


        }

        public static AngleList routplanning(double angle1, double angle2, double angle3, double angle4, double distance, int pointsnum)
        {

            MotorAngle origmotor = new MotorAngle(angle1, angle2, angle3, angle4);

            calcu(origmotor, distance, pointsnum);

            decimal[,] data = new decimal[100, 2];


            StreamReader angleFileReader = new StreamReader("moveangle.txt");


            for (int linenum = 0; !angleFileReader.EndOfStream; linenum++)
            {
                string[] strSplit = angleFileReader.ReadLine().Split(',');

                data[linenum, 0] = Convert.ToDecimal(strSplit[0]);
                data[linenum, 1] = Convert.ToDecimal(strSplit[1]);
            }

            angleFileReader.Close();


            StreamWriter angleFileWriter = new StreamWriter("moveangle.txt");

            AngleList list = new AngleList(90 - (decimal)angle1, (decimal)angle2, 180 - (decimal)angle3, (decimal)angle4-180);
            Angle currnode = list.headAngle;

            decimal tmpangle = (decimal)angle4;


            for (int i = 0; i < pointsnum; i++)
            {
                if (i == 0)
                {
                    tmpangle += ((data[i, 0] - (decimal)angle1) + (data[i, 1] - (decimal)angle3));
                }
                else
                {
                    tmpangle += ((data[i, 0] - data[i - 1, 0]) + (data[i, 1] - data[i - 1, 1]));
                }

                angleFileWriter.WriteLine("{0:f12},{1:f12},{2:f12}", 90 - data[i, 0], 180 - data[i, 1],  tmpangle-180);

                currnode.creatNextNode(90 - data[i, 0], (decimal)angle2, 180 - data[i, 1], tmpangle-180);
                currnode = currnode.nextangle;

            }


            angleFileWriter.Close();

            return list;

        }

    }

}
