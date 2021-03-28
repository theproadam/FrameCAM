using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using STL;
using System.IO;

namespace FrameCAM
{
    struct Nailable
    {
        public Vector3 nailPosition;
        public int nailRotation;
        public float nailOrder;
        
        public Nailable(Vector3 nP, int nRot, float pos)
        {
            nailPosition = nP;
            nailRotation = nRot;
            nailOrder = pos;
        }
    }

    class Program
    {
        static bool CheckForNailingPos = false;
        static Vector3 PickupPosition = new Vector3(0, 2, 0.62f); //note z position is different!!
        static Vector3 BeamOffset = new Vector3(0.15f, 0.14f, 0);

        static bool CheckNailPos(Triangle input)
        {
            int posC = 0;

            if (Math.Abs(input.vertex1.y - 0) < 0.01f) posC++;
            if (Math.Abs(input.vertex2.y - 0) < 0.01f) posC++;
            if (Math.Abs(input.vertex3.y - 0) < 0.01f) posC++;

            if (posC != 1) return false;

            posC = 0;

            if (input.vertex1.y < -0.01f) posC++;
            if (input.vertex2.y < -0.01f) posC++;
            if (input.vertex3.y < -0.01f) posC++;

            if (posC != 2) return false;

            return true;
        }

        static bool CheckYCoord(Triangle input)
        {
            if (Math.Abs(input.vertex1.y - 0) > 0.01f) return true;
            if (Math.Abs(input.vertex2.y - 0) > 0.01f) return true;
            if (Math.Abs(input.vertex3.y - 0) > 0.01f) return true;

            else return false;
        }

        static float getZValue(Triangle input, out float ZSize)
        {
            float Z = 0;
            ZSize = 0;

            bool P2P1 = (Math.Abs(input.vertex2.x - input.vertex1.x) < 0.01f);
            bool P3P2 = (Math.Abs(input.vertex3.x - input.vertex2.x) < 0.01f);
            bool P1P3 = (Math.Abs(input.vertex1.x - input.vertex3.x) < 0.01f);

            if (P2P1 == P1P3 == P1P3) throw new Exception("Invalid Geometry!");

            if (P2P1 && P1P3) throw new Exception("Invalid Geometry!");
            if (P2P1 && P3P2) throw new Exception("Invalid Geometry!");
            if (P3P2 && P1P3) throw new Exception("Invalid Geometry!");

            if (P2P1)
            {
                float yv = (input.vertex2.z - input.vertex1.z) / 2f;
                Z = yv + input.vertex1.z;

                ZSize = Math.Abs(input.vertex2.z - input.vertex1.z);
            }
            else if (P3P2)
            {
                float yv = (input.vertex3.z - input.vertex2.z) / 2f;
                Z = yv + input.vertex2.z;

                ZSize = Math.Abs(input.vertex3.z - input.vertex2.z);
            }
            else if (P1P3)
            {
                float yv = (input.vertex1.z - input.vertex3.z) / 2f;
                Z = yv + input.vertex3.z;

                ZSize = Math.Abs(input.vertex1.z - input.vertex3.z);
            }

            return Z;
        }

        static float getXValue(Triangle input, out float XSize)
        {
            float X = 0;
            XSize = 0;

            bool P2P1 = (Math.Abs(input.vertex2.z - input.vertex1.z) < 0.01f);
            bool P3P2 = (Math.Abs(input.vertex3.z - input.vertex2.z) < 0.01f);
            bool P1P3 = (Math.Abs(input.vertex1.z - input.vertex3.z) < 0.01f);

            if (P2P1 == P1P3 == P1P3) throw new Exception("Invalid Geometry!");

            if (P2P1 && P1P3) throw new Exception("Invalid Geometry!");
            if (P2P1 && P3P2) throw new Exception("Invalid Geometry!");
            if (P3P2 && P1P3) throw new Exception("Invalid Geometry!");

            if (P2P1)
            {
                float yv = (input.vertex2.x - input.vertex1.x) / 2f;
                X = yv + input.vertex1.x;

                XSize = Math.Abs(input.vertex2.x - input.vertex1.x);
            }
            else if (P3P2)
            {
                float yv = (input.vertex3.x - input.vertex2.x) / 2f;
                X = yv + input.vertex2.x;

                XSize = Math.Abs(input.vertex3.x - input.vertex2.x);
            }
            else if (P1P3)
            {
                float yv = (input.vertex1.x - input.vertex3.x) / 2f;
                X = yv + input.vertex3.x;

                XSize = Math.Abs(input.vertex1.x - input.vertex3.x);
            }

            return X;
        }

        static bool isPresent(List<Vector3> arr, float X, float Z)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                if (Math.Abs(arr[i].x - X) <= 0.01f && Math.Abs(arr[i].y - Z) <= 0.01f)
                {
                    return true;
                }
            }

            return false;
        }

        static Nailable ComputeNail(Triangle input)
        {
            Vector3 ComputeVertex = new Vector3(0, 0, 0), V2 = new Vector3(0, 0, 0), V3 = new Vector3(0, 0, 0);
            int Rot = -1; //CCW

            if (Math.Abs(input.vertex1.y - 0) < 0.01f) { ComputeVertex = input.vertex1; V2 = input.vertex2; V3 = input.vertex3; }
            else if (Math.Abs(input.vertex2.y - 0) < 0.01f) { ComputeVertex = input.vertex2; V2 = input.vertex3; V3 = input.vertex1; }
            else if (Math.Abs(input.vertex3.y - 0) < 0.01f) { ComputeVertex = input.vertex3; V2 = input.vertex1; V3 = input.vertex2; }


            if (Math.Abs(ComputeVertex.x - V2.x) < 0.01f)
            {
                if (ComputeVertex.z > V2.z) Rot = 0;
                else Rot = 2;
            }
            else if (Math.Abs(ComputeVertex.z - V2.z) < 0.01f)
            {
                if (ComputeVertex.x > V2.x) Rot = 1;
                else Rot = 3;
            }
            else throw new Exception("Error In Geometry!");

            if (Math.Abs(ComputeVertex.x - 0) < 0.01f) ComputeVertex.x = 0;
            if (Math.Abs(ComputeVertex.y - 0) < 0.01f) ComputeVertex.y = 0;

            return new Nailable(ComputeVertex, Rot, V2.y < V3.y ? V2.y : V3.y);
        }

        static void WriteNailTargets(List<Nailable> nailPos, List<string> WriteBuffer)
        {
            List<List<Nailable>> nailList = new List<List<Nailable>>();

            float NailIndex = 0;
            for (int i = 0; i < nailPos.Count; i++)
            {
                if (Math.Abs(nailPos[i].nailOrder - NailIndex) < 0.01f)
                {
                    nailList[nailList.Count - 1].Add(nailPos[i]);
                    NailIndex = nailPos[i].nailOrder;
                }
                else
                {
                    nailList.Add(new List<Nailable>());
                    nailList[nailList.Count - 1].Add(nailPos[i]);
                    NailIndex = nailPos[i].nailOrder;
                }
            }


            for (int i = 0; i < nailList.Count; i++)
            {
                if (nailList[i].Count > 2)
                {
                    if (Math.Abs(nailList[i][0].nailPosition.x - nailList[i][0].nailPosition.x) < 0.01f)
                    { //move on z
                        nailList[i].Sort(delegate(Nailable x, Nailable y) { return x.nailPosition.z.CompareTo(y.nailPosition.z); });
                    }
                    else
                    {
                        nailList[i].Sort(delegate(Nailable x, Nailable y) { return x.nailPosition.x.CompareTo(y.nailPosition.x); });
                    }
                }
            }


            for (int i = 0; i < nailList.Count; i++)
            {
                float TRot = nailList[i][0].nailRotation * (float)Math.PI / 2f;
                //goto first pos->
                WriteBuffer.Add("\n\n //NAIL GUN ACTIVATING");
                WriteBuffer.Add("Instructions.Add(new GCode(" + nailList[i][0].nailPosition.z + "f, " + nailList[i][0].nailPosition.x + "f, 0, " + TRot + "f)); //Go To Piece Pickup");
                
                float XOFFSET = 0, MOFFSETX = 0;
                float ZOFFSET = 0, MOFFSETZ = 0;

                if (nailList[i][0].nailRotation == 2) { XOFFSET = 0.2518f; MOFFSETX = 0.01f; }
                if (nailList[i][0].nailRotation == 0) { XOFFSET = 0.028f; MOFFSETX = -0.01f; }

                if (nailList[i][0].nailRotation == 3) { ZOFFSET = 0.2518f; MOFFSETZ = 0.01f; }
                if (nailList[i][0].nailRotation == 1) { ZOFFSET = 0.028f; MOFFSETZ = -0.01f; }


                for (int j = 0; j < nailList[i].Count; j++)
                {
                    Nailable Target = nailList[i][j];
                    Target.nailPosition += BeamOffset;

                    //get ready pos->
                    WriteBuffer.Add("Instructions.Add(new GCode(" + (Target.nailPosition.z + XOFFSET + MOFFSETX) + "f, " + (Target.nailPosition.x + ZOFFSET + MOFFSETZ) + "f, " + "0.31f" + ", " + TRot + "f)); //Go To Piece Pickup");
                    

                    //push nail in pos ->
                    WriteBuffer.Add("Instructions.Add(new GCode(" + (Target.nailPosition.z + XOFFSET) + "f, " + (Target.nailPosition.x + ZOFFSET) + "f, " + "0.31f" + ", " + TRot + "f)); //Go To Piece Pickup");

                    //wait timetick ->
                    WriteBuffer.Add("Instructions.Add(new GCode(true));");


                    //retract
                    WriteBuffer.Add("Instructions.Add(new GCode(" + (Target.nailPosition.z + XOFFSET + MOFFSETX) + "f, " + (Target.nailPosition.x + ZOFFSET + MOFFSETZ) + "f, " + "0.31f" + ", " + TRot + "f)); //Go To Piece Pickup");
                    
                    //go to next
                    WriteBuffer.Add("Instructions.Add(new GCode(" + (Target.nailPosition.z + XOFFSET + MOFFSETX) + "f, " + (Target.nailPosition.x + ZOFFSET + MOFFSETZ) + "f, " + "0.375f" + ", " + TRot + "f)); //Go To Piece Pickup");

                    //push nail in
                    WriteBuffer.Add("Instructions.Add(new GCode(" + (Target.nailPosition.z + XOFFSET) + "f, " + (Target.nailPosition.x + ZOFFSET) + "f, " + "0.375f" + ", " + TRot + "f)); //Go To Piece Pickup");

                    //wait
                    WriteBuffer.Add("Instructions.Add(new GCode(true));");


                    //retract
                    WriteBuffer.Add("Instructions.Add(new GCode(" + (Target.nailPosition.z + XOFFSET + MOFFSETX) + "f, " + (Target.nailPosition.x + ZOFFSET + MOFFSETZ) + "f, " + "0.375f" + ", " + TRot + "f)); //Go To Piece Pickup");

                    if (j == nailList[i].Count - 1)
                        WriteBuffer.Add("Instructions.Add(new GCode(" + (Target.nailPosition.z + XOFFSET + MOFFSETX) + "f, " + (Target.nailPosition.x + ZOFFSET + MOFFSETZ) + "f, " + "0f" + ", " + TRot + "f)); //Go To Piece Pickup");
                }


            }
            
        }


        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "FrameCAM";

            OpenFileDialog oDialog = new OpenFileDialog();
            oDialog.Filter = "STL Files |*.stl|All Files |*.*";

            if (oDialog.ShowDialog() != DialogResult.OK)
                return;

            Console.Write("Import -> ");

            STLImporter SImport = new STLImporter(oDialog.FileName);
            Console.WriteLine("OK");

            List<Triangle> usedTris = new List<Triangle>();
            List<Triangle> usedNail = new List<Triangle>();
            

            for (int i = 0; i < SImport.AllTriangles.Length; i++)
            {
                if (CheckNailPos(SImport.AllTriangles[i]))
                    usedNail.Add(SImport.AllTriangles[i]);

                if (!CheckYCoord(SImport.AllTriangles[i]))
                    usedTris.Add(SImport.AllTriangles[i]);
            }

            if (usedTris.Count % 2 != 0)
            {
                Console.WriteLine("Invalid Geometry.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Detected " + usedTris.Count / 2 + " Wood Pieces!");

            List<Vector3> beamPos = new List<Vector3>();
            List<Vector3> beamSiz = new List<Vector3>();

            Console.WriteLine("Calculating Beam Placements");
            for (int i = 0; i < usedTris.Count; i++)
            {
                float ZSize = 0;
                float ZPos = getZValue(usedTris[i], out ZSize);

                float XSize = 0;
                float XPos = getXValue(usedTris[i], out XSize);

                bool ZW = (Math.Abs(ZSize - 0.0381f) < 0.01f);
                bool XW = (Math.Abs(XSize - 0.0381f) < 0.01f);

                if (ZW && XW) throw new Exception("Invalid Geometry!");
                if (!ZW && !XW) throw new Exception("Invalid Geometry!");


                if (!isPresent(beamPos, XPos, ZPos))
                {
                    beamPos.Add(new Vector3(XPos, ZPos, 0));
                    beamSiz.Add(new Vector3(XSize, ZSize, 0));
                }              
            }

            List<Nailable> nailPos = new List<Nailable>();
            Console.WriteLine("Calculating Nailer");

            for (int i = 0; i < usedNail.Count; i++)
            {
                nailPos.Add(ComputeNail(usedNail[i]));
            }
         
            nailPos.Sort(delegate(Nailable x, Nailable y) { return x.nailOrder.CompareTo(y.nailOrder); });
            
            Console.WriteLine("CAM Processing OK\nPlease select a path to save gcode file");

            SaveFileDialog sDialog = new SaveFileDialog();
            sDialog.Filter = "Text Files |*.txt| All Files|*.*";

            if (sDialog.ShowDialog() != DialogResult.OK)
                return;

            List<string> GInstr = new List<string>();

            Vector3 GoUpPos = new Vector3(0, 0, 0);

            for (int i = 0; i < beamPos.Count; i++)
            {
                beamPos[i] = beamPos[i] + BeamOffset;
            }

            for (int i = 0; i < beamPos.Count; i++)
            {
                bool SW = Math.Abs(beamSiz[i].x - 0.0381f) < 0.01f;
                float ROT = SW ? 1.57f : 0f;

                //release previous piece and spawn the new piece
                GInstr.Add("Instructions.Add(new GCode(false, " + (SW ? beamSiz[i].y : beamSiz[i].x) + "f)); //Release Previous Piece");

                //go up incase of last piece
                GInstr.Add("Instructions.Add(new GCode(" + GoUpPos.y + "f, " + GoUpPos.x + "f, 0f, " + GoUpPos.z + "f)); //Go Up");

                //Get The Piece
                GInstr.Add("Instructions.Add(new GCode(0, 2, 0, 0)); //Go To Piece Pickup");
                GInstr.Add("Instructions.Add(new GCode(0, 2, 0.62f, 0)); //Lower To Piece");

                //Pickup the piece
                GInstr.Add("Instructions.Add(new GCode(true, 12345f)); //Pickup Piece");
                GInstr.Add("Instructions.Add(new GCode(0, 2, 0, 0)); //Go up");

                //goto pos
                GInstr.Add("Instructions.Add(new GCode(" + beamPos[i].y + "f, " + beamPos[i].x + "f, 0f, " + ROT + "f)); //Goto Piece Pos");

                //Lower Piece
                GInstr.Add("Instructions.Add(new GCode(" + beamPos[i].y + "f, " + beamPos[i].x + "f, 0.49f, " + ROT + "f)); //Lower the piece");
                GoUpPos = new Vector3(beamPos[i].x, beamPos[i].y, ROT);

                //loop end repeat ->
            }

            //release last piece and go home
            GInstr.Add("Instructions.Add(new GCode(false, " + "0" + "f)); //Release Previous Piece");
            GInstr.Add("Instructions.Add(new GCode(" + GoUpPos.y + "f, " + GoUpPos.x + "f, 0f, " + GoUpPos.z + "f)); //Go Up");
            GInstr.Add("Instructions.Add(new GCode(0, 0, 0, 0)); //go home");


            WriteNailTargets(nailPos, GInstr);
          


            File.WriteAllLines(sDialog.FileName, GInstr.ToArray());
            Console.WriteLine("File Written!");
            Console.ReadLine();
            
        }
    }
}
