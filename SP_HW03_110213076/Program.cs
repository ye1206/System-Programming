using System;
using System.IO;

namespace SP_HW03_110213076
{
    class Program
    {
        static void Main(string[] args)
        {
            #region global
            char[] delim = { ',', '\t', ';', '\r', ' ' }; //Record delimiter
            int i = 0;
            string line;
            int lineCount = 0;
            int hex; //store the opcode value in hex
            int opeRES = 0; //store the operand value of RESB or RESW
            int loc = 0; //store the location
            List<string> sourceCode = new List<string>(); //store the source code
            string[] LMO = new string[3]; //Label, Mnemonic, Operand
            Dictionary<string, string> opcodeTable = new Dictionary<string, string>(); //store the opcode table
            Dictionary<string, string> symbolTable = new Dictionary<string, string>(); //store the symbol table
            #endregion global

            #region read the file
            op.readOp(opcodeTable);

            StreamReader fileReader = File.OpenText("testprog.S");

            while (!fileReader.EndOfStream) //record the code into a list
            {
                i++;
                line = fileReader.ReadLine(); //record every line read in
                if (line.Contains("END")) //if the line contains "END" or "end", break the loop
                {
                    sourceCode.Add(line); //add the line to the list
                    break;
                } //if
                sourceCode.Add(line);
            } //while

            fileReader.Close();
            #endregion read the file

            i = 1; //reset i to 1 as line number

            while (lineCount <= sourceCode.Count)
            {
                if (sourceCode[lineCount].StartsWith(".") || sourceCode[lineCount].StartsWith(" ") || sourceCode[lineCount] == "")
                {
                    Console.WriteLine($"{i}: " + sourceCode[lineCount]);
                    lineCount++;
                    i++;
                    continue;
                } //if
                else
                {
                    LMO = sourceCode[lineCount].Split(delim, 3); //split the line into Label, Mnemonic, Operand

                    if (LMO[2].Contains(".comment"))
                        LMO[2] = LMO[2].Replace(".comment", "").Trim(); //remove the .comment from the operand
                    else if (LMO[2].Contains(". comment"))
                        LMO[2] = LMO[2].Replace(". comment", "").Trim(); //remove the . comment from the operand

                    if (!opcodeTable.ContainsKey(LMO[1]))
                    {
                        if (LMO[1] == "START")
                        {
                            Console.WriteLine($"{i}: " + sourceCode[lineCount]);
                            Console.WriteLine("Program starts. Program name: " + LMO[0]);

                            loc = int.Parse(LMO[2], System.Globalization.NumberStyles.HexNumber);
                            symbolTable.Add(LMO[0], loc.ToString("X"));
                        } //if
                        else if (LMO[1] == "END")
                        {
                            Console.WriteLine($"{i}: " + sourceCode[lineCount]);
                            Console.WriteLine("Program ends.");

                            loc += 3;
                            break;
                        } //else if
                        else if (LMO[1] == "RESB") //if RESB
                        {
                            Console.WriteLine($"{i}: " + sourceCode[lineCount]);
                            Console.WriteLine("Pseudo code. Reserve " + LMO[2] + " bytes.");
                            Console.WriteLine("Label: " + LMO[0] + " Mnemonic: " + LMO[1] + " Operand: " + LMO[2]);

                            if (sourceCode[lineCount - 1].Contains("RESW")) //if the previous line contains RESW
                            {
                                loc = loc + opeRES * 3; //add the operand value to the location
                                symbolTable.Add(LMO[0], loc.ToString("X")); 
                            }
                            else if (sourceCode[lineCount - 1].Contains("RESB")) //if the previous line contains RESB
                            {
                                loc = loc + opeRES;
                                symbolTable.Add(LMO[0], loc.ToString("X"));
                            }
                            else //if the previous line is not RESW or RESB
                            {
                                loc += 3;
                                symbolTable.Add(LMO[0], loc.ToString("X"));
                            }

                            opeRES = int.Parse(LMO[2], System.Globalization.NumberStyles.HexNumber); //store the operand value
                        } //else if
                        else if (LMO[1] == "RESW") //if RESW
                        {
                            Console.WriteLine($"{i}: " + sourceCode[lineCount]);
                            Console.WriteLine("Pseudo code. Reserve " + LMO[2] + " words.");
                            Console.WriteLine("Label: " + LMO[0] + " Mnemonic: " + LMO[1] + " Operand: " + LMO[2]);

                            
                            if (sourceCode[lineCount - 1].Contains("RESW"))
                            {
                                loc = loc + opeRES * 3; 
                                symbolTable.Add(LMO[0], loc.ToString("X"));
                            }
                            else if (sourceCode[lineCount - 1].Contains("RESB"))
                            {
                                loc = loc + opeRES;
                                symbolTable.Add(LMO[0], loc.ToString("X"));
                            }
                            else
                            {
                                loc += 3;
                                symbolTable.Add(LMO[0], loc.ToString("X"));
                            }

                            opeRES = int.Parse(LMO[2]);
                            Console.WriteLine("opeRES: " + opeRES);
                        } //else if
                        else if (LMO[1] == "BYTE")
                        {
                            Console.WriteLine($"{i}: " + sourceCode[lineCount]);
                            Console.WriteLine("Pseudo code.");
                            Console.WriteLine("Label: " + LMO[0] + " Mnemonic: " + LMO[1] + " Operand: " + LMO[2]);

                        } //else if
                        else if (LMO[1] == "WORD")
                        {
                            Console.WriteLine($"{i}: " + sourceCode[lineCount]);
                            Console.WriteLine("Pseudo code.");
                            Console.WriteLine("Label: " + LMO[0] + " Mnemonic: " + LMO[1] + " Operand: " + LMO[2]);

                        } //else if
                        else
                        {
                            Console.WriteLine("Invalid!");
                            System.Environment.Exit(0);
                        } //else
                    } //if mnemonic code is not in the opcode table
                    else
                    {
                        Console.WriteLine($"{i}: " + sourceCode[lineCount]);
                        Console.WriteLine("Label: " + LMO[0] + " Mnemonic: " + LMO[1] + " Operand: " + LMO[2]);

                        if (LMO[2].Contains(", X")) //index addressing or not
                            Console.WriteLine("**Index Addressing**");

                        if (LMO[0] != "") 
                        {
                            if (sourceCode[lineCount - 1].Contains("START")) //if the previous line contains START
                                symbolTable.Add(LMO[0], loc.ToString("X"));
                            else
                            {
                                loc += 3;
                                symbolTable.Add(LMO[0], loc.ToString("X"));
                            } //normal code
                        } //if exists label
                        else
                        {
                            loc += 3;
                        } // if label is null
                    } //else

                    lineCount++;
                    i++;
                }
            } //while

            Console.WriteLine();
            Console.WriteLine("**Symbol Table**");
            foreach (KeyValuePair<string, string> kvp in symbolTable)
            {
                Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value);
            } //foreach

            Console.Read();
        } //main
    }   
}
