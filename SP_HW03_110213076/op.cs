using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_HW03_110213076
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
                else
                    break;
            } //while
        } //readOp
    }
}
