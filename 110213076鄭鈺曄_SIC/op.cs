using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _110213076_Final
{
    public class op
    {
        public static void readOp(Dictionary<string, string> opcodeTable)
        {
            string line;
            char[] delim = { ',', '\t', ';', '\r', ' ' }; //Record delimiter
            StreamReader fileReader = File.OpenText("opCode.txt");
            while (!fileReader.EndOfStream)
            {
                line = fileReader.ReadLine();

                if (!string.IsNullOrEmpty(line))
                {
                    string[] inputField = line.Split(delim);
                    opcodeTable.Add(inputField[0], inputField[1]);
                } //if
            } //while

            fileReader.Close();
        } //readOp
    }
}
