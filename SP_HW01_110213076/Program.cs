using System;
using System.Collections.Generic;
using System.IO;

namespace SP_HW01_110213076
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0;
            string mnem;
            Dictionary<string, string> opcodeTable = new Dictionary<string, string>(); //store the opcode table

            StreamReader fileReader = File.OpenText("opCode.txt");
            char[] delim = {',', '\t', ';', '\r', ' '}; //Record delimiter
            while(!fileReader.EndOfStream)
            {      
                string line = fileReader.ReadLine();

                if (!string.IsNullOrEmpty(line))
                {
                    string[] inputField = line.Split(delim);
                    opcodeTable.Add(inputField[0], inputField[1]); //add the mnemonic code and opcode to the dictionary
                } //if
                else
                    break;
            }

            fileReader.Close();
            
            Console.Write("Enter the mnemonic code: ");
            mnem = Console.ReadLine();

            if (opcodeTable.ContainsKey(mnem))
                Console.WriteLine("The opCode is: " + opcodeTable[mnem]);
            else
                Console.WriteLine("Invalid mnemonic code!");
        } //main
    }
}




