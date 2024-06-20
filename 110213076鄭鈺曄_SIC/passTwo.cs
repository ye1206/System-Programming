using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace _110213076_Final
{
    public class passTwo
    {
        public static string pattern = @"[,X]"; //pattern to remove X
        public static string[] pseudo = { "START", "END", "RESB", "RESW", "BYTE", "WORD" }; //Pseudo code
        //public static char[] delim = { ',', '\t', ';', '\r', ' ' }; //Record delimiter
        public static void UndefinedOperand(Dictionary<string, string> symbolTable, List<string> inter, ref bool isError)
        {
            int result;
            int sourceCount = 0;
            int interCount = 0;
            string[] sep = new string[7];

            while (interCount < inter.Count)
            {
                sep = inter[interCount].Split('\t', 7); //split the line into 6 parts [loc /label /mnem/ operand/ opcode Value/ addressing / line]
                if (int.TryParse(sep[3], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result) || symbolTable.ContainsKey(sep[3]) || sep[3] == "***")
                    interCount++;
                else if (sep[3].StartsWith("C") || sep[3].StartsWith("X"))
                {
                    Match match = Regex.Match(sep[3], @"'([^']*)'"); //match the operand value
                    if (match.Success)
                        interCount++;
                }
                else if (sep[3].Contains(",X"))
                {
                    sep[3] = Regex.Replace(sep[3], pattern, "");
                    if (!symbolTable.ContainsKey(sep[3])) //if the operand exists in the symbol table
                    {
                        Console.WriteLine($"Line {sep[6]} in Pass 2: \tUndefined Operand!");
                        isError = true;
                    } //else
                    interCount++;
                } //else if 
                else //if the operand does not exist in the symbol table
                {
                    Console.WriteLine($"Line {sep[6]} in Pass 2: Undefined Operand!");
                    isError = true;
                    interCount++;
                } //else
            } //while
        } //UndefinedOperand

        public static void GenerateCode(string opcodeValue, string operand, Dictionary<string, string> symbolTable, string mnem, List<string> output, string addr)
        {
            if (mnem == pseudo[2] || mnem == pseudo[3]) //if the mnem is RESB or RESW
            {
                output.Add("");
                return;
            }
            else if (mnem == pseudo[4]) //if the mnem is BYTE
            {
                Match match = Regex.Match(operand, @"'([^']*)'"); //match the operand value
                string temp = "";
                if (match.Success)
                    temp = match.Groups[1].Value; //store the operand value

                if (operand.StartsWith("C"))
                {
                    string ascii = "";

                    foreach (char c in temp)
                    {
                        int asciiCode = (int)c;
                        ascii += asciiCode.ToString("X2"); //convert the operand value to hex
                    }
                    output.Add(ascii); //add the operand value to the output list
                }
                else if (operand.StartsWith("X"))
                    output.Add(temp); //add the operand value to the output list  
            } //else if
            else if (mnem == pseudo[5]) //if the mnem is WORD
            {
                operand = int.Parse(operand).ToString("X").PadLeft(6, '0'); //convert the operand value to hex
                output.Add(operand); //add the operand to the output list
            } //else if
            else if (mnem == "RSUB")
            { 
                operand = opcodeValue.PadRight(6, '0'); //store the opcode value
                output.Add(operand); //add the opcode value to the output list
            } //else if
            else if (addr == "indexed") //if the operand contains X
            {
                int commaIndex = operand.IndexOf(',');
                if (commaIndex >= 0) // 提取逗號前的內容
                    operand = operand.Substring(0, commaIndex);

                operand = (Convert.ToInt32(symbolTable[operand], 16) + 32768).ToString("X"); //store the operand value with X value
                operand = opcodeValue + operand; //store the operand value
                output.Add(operand.PadLeft(6, '0')); //add the operand to the output list
            } //else if
            else
            {
                operand = symbolTable[operand]; //store the operand value
                output.Add((opcodeValue + operand).PadLeft(6, '0')); //add the opcode value and operand value to the output list
            } //else
        } // GenerateCode

        public static void WriteFile(List<string> output, StreamWriter objectFile, List<string> inter, ref bool error)
        {
            string[] sep = inter[0].Split('\t'); //split the first line
            string startAddress = sep[0].PadLeft(6, '0'); //store the start address
            int count = 0;
            int maxLength = 30; //max length of the text record
            int length = 0; //length of the text record
            List<string> current = new List<string>(); //store the current text record

            while (count < output.Count && error == false)
             {
                if (output[count] == "")
                {
                    if (output[count + 1] != "")
                    {
                        Trecord(objectFile, startAddress, length, current); //write the file

                        current.Clear(); //clear current list
                        length = 0; //initialize the length
                    }
                } //if sourCode is RESW or RESB
                else
                {
                    if (current.Count == 0)
                    {
                        sep = inter[count + 1].Split('\t'); //find the next start address
                        startAddress = sep[0].PadLeft(6, '0');
                    }

                    if (length + 3 > maxLength)
                    {
                        Trecord(objectFile, startAddress, length, current);

                        sep = inter[count + 1].Split('\t');
                        startAddress = sep[0].PadLeft(6, '0');
                        length = 0;
                        current.Clear();

                        if (inter[count + 1].Contains(pseudo[4])) //if is BYTE
                        {
                            sep = inter[count + 1].Split('\t');
                            Match match = Regex.Match(sep[3], @"'([^']*)'"); //match the operand value
                            string temp = "";
                            if (match.Success)
                                temp = match.Groups[1].Value; //store the operand value

                            if (sep[3].StartsWith("C")) //if the operand starts with C
                                length += temp.Length; //store the length of the operand
                            else if (sep[3].StartsWith("X")) //if the operand starts with X
                                length += temp.Length / 2; //store the length of the operand in bytes
                        }
                        else
                            length += 3;

                        current.Add(output[count]); //add the object code to list
                    }
                    else if (length < maxLength) // if current length < max length
                    {
                        current.Add(output[count]);

                        if (inter[count + 1].Contains(pseudo[4])) //if is BYTE
                        {
                            sep = inter[count + 1].Split('\t');
                            Match match = Regex.Match(sep[3], @"'([^']*)'"); //match the operand value
                            string temp = "";
                            if (match.Success)
                                temp = match.Groups[1].Value; //store the operand value

                            if (sep[3].StartsWith("C")) //if the operand starts with C
                                length += temp.Length; //store the length of the operand
                            else if (sep[3].StartsWith("X")) //if the operand starts with X
                                    length += temp.Length / 2; //store the length of the operand in bytes
                        }
                        else
                            length += 3;
                    } //if
                    else
                    {
                        Trecord(objectFile, startAddress, length, current);
                        
                        sep = inter[count + 1].Split('\t');
                        startAddress = sep[0].PadLeft(6, '0');
                        length = 0;
                        current.Clear();
                        current.Add(output[count]);
                        length += 3;
                    } //else
                } //else
                    count++;
             } //while

            if (count == output.Count) //if is the last object code
                Trecord(objectFile, startAddress, length, current);
        } //WriteFile

        public static void Trecord(StreamWriter objectFile, string startAddress, int length, List<string> current)
        {
            objectFile.Write($"T^{startAddress}^{length.ToString("X2")}");
            foreach (string s in current)
                objectFile.Write($"^{s}");
            objectFile.WriteLine();
        }

        public static void mRecord(List<string> inter, StreamWriter objectFile)
        {
            int i = 0;
            //string startLabel = inter[0].Split('\t')[1]; //store the start label
            HashSet<string> pseudoSet = new HashSet<string>(pseudo); //store the pseudo code
            for (i = 0; i < inter.Count; i++)
            {
                string[] sep = inter[i].Split('\t');
                int loc = int.Parse(sep[0], System.Globalization.NumberStyles.HexNumber);

                if (!pseudoSet.Contains(sep[2]) && sep[2] != "RSUB") //if mnem != pseudo code or RSUB
                {
                    loc += 1; //modified location
                    objectFile.WriteLine($"M^{loc.ToString("X").PadLeft(6, '0')}^04"); 
                } //if
            } //for
        } //mRecord

        //public static void relocationBit(List<string> sourceCode, StreamWriter objectFile, List<string> output)
        //{
        //    int relocation = 0; //relocation bit
        //    int i = 0;
        //    int maxLength = 30; //max length of the text record
            
        //    string[] sep = sourceCode[0].Split('\t'); //split the first line
        //    string startAddress = sep[0].PadLeft(6, '0'); //store the start address
        //    int count = 0;
        //    int length = 0; //length of the text record
        //    List<string> current = new List<string>(); //store the current text record
        //    List<int> bit = new List<int>(); //store the relocation bit

        //    while (count < output.Count)
        //    {
        //        if (output[count] == "")
        //        {
        //            if (output[count + 1] != "")
        //            {
        //                objectFile.Write($"T^{startAddress}^{length.ToString("X2")}^"); //write the file
        //                for (i = 0; i < bit.Count; i++)
        //                    relocation = bit[i] << 1;
        //                objectFile.Write(relocation.ToString("X3"));
        //                foreach (string s in current)
        //                    objectFile.Write($"^{s}");
        //                objectFile.WriteLine();

        //                current.Clear(); //clear current list
        //                length = 0; //initialize the length
        //                bit.Clear(); //clear the relocation bit
        //                relocation = 0; //initialize the relocation bit
        //            } //if the mnem is RESW or RESB
        //            count++;
        //            continue;
        //        } //if
        //        else
        //        {
        //            if (current.Count == 0)
        //            {
        //                sep = sourceCode[count + 1].Split('\t'); //find the next start address
        //                startAddress = sep[0].PadLeft(6, '0');
        //            }

        //            if (length + 3 > maxLength || bit.Count + 1 > 12)
        //            {
        //                objectFile.Write($"T^{startAddress}^{length.ToString("X2")}");
        //                foreach (string s in current)
        //                    objectFile.Write($"^{s}");
        //                objectFile.WriteLine();

        //                sep = sourceCode[count + 1].Split('\t');
        //                startAddress = sep[0].PadLeft(6, '0');
        //                length = 0;
        //                current.Clear();
        //                bit.Clear();
        //                relocation = 0;
        //                current.Add(output[count]); //add the object code to list
        //                length += 3; //add length
        //                bit.Add(1);
        //            }
        //            else if (length < maxLength) // if current length < max length
        //            {
        //                current.Add(output[count]);

        //                if (sourceCode[count + 1].Contains(pseudo[4])) //if is BYTE
        //                {
        //                    sep = sourceCode[count + 1].Split('\t');

        //                    if (sep[3].StartsWith("C")) //if the operand starts with C
        //                    {
        //                        Match match = Regex.Match(sep[3], @"'([^']*)'"); //match the operand value
        //                        string temp = "";
        //                        if (match.Success)
        //                            temp = match.Groups[1].Value; //store the operand value
        //                        length += temp.Length; //store the length of the operand
        //                    } //if C
        //                    else if (sep[3].StartsWith("X")) //if the operand starts with X
        //                    {
        //                        Match match = Regex.Match(sep[3], @"'([^']*)'"); //match the operand value
        //                        string temp = "";
        //                        if (match.Success)
        //                            temp = match.Groups[1].Value; //store the operand value
        //                        length += temp.Length / 2; //store the length of the operand in bytes
        //                    } //else if X
        //                }
        //                else
        //                    length += 3;
        //            } //if
        //            else
        //            {
        //                objectFile.Write($"T^{startAddress}^{length.ToString("X2")}");
        //                foreach (string s in current)
        //                    objectFile.Write($"^{s}");
        //                objectFile.WriteLine();

        //                sep = sourceCode[count + 1].Split('\t');
        //                startAddress = sep[0].PadLeft(6, '0');
        //                length = 0;
        //                current.Clear();
        //                current.Add(output[count]);
        //                length += 3;
        //            } //else
        //    } //else
        //        count++;
        //    } //while
        //} //relocationBit
    } //passTwo
}