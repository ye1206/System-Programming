using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Security;
using System.ComponentModel.Design;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

namespace _110213076_Final
{
    public class ErrorDetectedException : Exception
    {
        public ErrorDetectedException(string message) : base(message)
        {
        }
    }

    public class passOne
    {
        public static char[] delim = { ',', '\t', ';', '\r', ' ' }; //Record delimiter
        public static string[] pseudo = {"START", "END", "RESB", "RESW", "BYTE", "WORD"}; //Pesudo code
        public static string pattern = @"[. \t].*"; // pattern to remove the comment

        public static void sep_in_3(string[] LMO, Dictionary<string, string> opcodeTable, List<string> sourceCode, ref int lineCount, ref int loc,
            Dictionary<string, string> symbolTable, ref int opeRES, string addr, List<string> output, ref bool error, ref bool locHex)
        {
            string operand = "";
            if (LMO[2].Contains("X") || LMO[2].Contains("x"))
            {
                int Xcount = 1;

                if (LMO[2] == "x" || LMO[1].Contains("X") || LMO[1].Contains("x"))
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\n索引定址錯誤!");
                    error = true;
                    return;
                }

                if (LMO[2].Count(c => char.ToUpper(c) == 'X') > Xcount || LMO[2].Count(c => c == ',') > Xcount) //if the X or the comma is more than 1
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\n索引定址錯誤!");
                    error = true;
                    return;
                }
                else
                    LMO[2] = Regex.Replace(LMO[2], pattern, ",X");
            } //if

            if (LMO[1].Contains(",") || LMO[2].Contains(","))
            {
                int Xcount = 1;
                if (LMO[1].Count(c => c == ',') + LMO[2].Count(c => c == ',') > Xcount) //if the comma is more than 1
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\n索引定址錯誤!");
                    error = true;
                    return;
                }
            } //if

            if (LMO[1] == pseudo[4])
                LMO[2] = LMO[2].Split('.')[0].Trim(); //remove the comment
            else
                LMO[2] = Regex.Replace(LMO[2], pattern, "").Trim(); //remove the comment

            if (isSamefor3(LMO, sourceCode, ref lineCount, ref error, opcodeTable) == false)
            {
                if (!opcodeTable.ContainsKey(LMO[1]))
                {
                    if (pseudo.Contains(LMO[1])) //if is pesudo code
                    {
                        if (LMO[1] == pseudo[0]) //if mnem is START
                        {
                            if (locCheck(LMO[2], ref locHex, ref loc) == false) //check start location is hex or not
                            {
                                Console.WriteLine($"Line {lineCount + 1} Error: {sourceCode[lineCount]}\nStart location is not in hex.");
                                return;
                            } //if
                            else
                                isExists(symbolTable, LMO[0], ref loc, ref lineCount, ref sourceCode, ref error);
                        }
                        else if (LMO[1] == pseudo[1]) //if mnem is END
                        {
                            if (LMO[0] != "") //if label exists
                            {
                                isPesudo(ref loc, ref opeRES, ref output);
                                isExists(symbolTable, LMO[0], ref loc, ref lineCount, ref sourceCode, ref error);
                            } //inner-if
                            else //if label is not exists
                                isPesudo(ref loc, ref opeRES, ref output);
                        } //else if
                        else if (LMO[1] == pseudo[2] || LMO[1] == pseudo[3] || LMO[1] == pseudo[5]) //if mnem is RESW or RESB
                        {
                            if (isDecimal(LMO[2]) == true) //if the operand is decimal
                            {
                                isPesudo(ref loc, ref opeRES, ref output);
                                isExists(symbolTable, LMO[0], ref loc, ref lineCount, ref sourceCode, ref error);
                                opeRES = int.Parse(LMO[2]); //store the operand value
                            }
                            else
                            {
                                Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\n Operand must be decimal");
                                error = true;
                                return;
                            } //else
                        } //if
                        else if (LMO[1] == pseudo[4]) //if mnem is BYTE
                        {
                            Match match = Regex.Match(LMO[2], @"'([^']*)'"); //match the operand value
                            string temp = "";
                            if (match.Success)
                                temp = match.Groups[1].Value; //store the operand value
                            else
                            {
                                Console.WriteLine($"Line {lineCount + 1}: {sourceCode[lineCount]}\nFail to get the operand.");
                                error = true;
                                return;
                            }

                            if (string.IsNullOrEmpty(temp))
                            {
                                Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nInvalid Operand! BYTE operand cannot be empty or only white space.");
                                error = true;
                                return;
                            }

                            if (LMO[2].StartsWith("C") || LMO[2].StartsWith("X")) //if the operand starts with C or X
                            {
                                if (LMO[2].StartsWith("C"))
                                {
                                    isPesudo(ref loc, ref opeRES, ref output);
                                    opeRES = temp.Length; //store the length of the operand
                                }
                                else
                                {
                                    string hex = @"\A[0-9a-fA-F]+\Z"; //pattern for hex
                                    if (Regex.IsMatch(temp, hex) == true) //if the operand is hex
                                    {
                                        if (temp.Length % 2 == 0)
                                        {
                                            isPesudo(ref loc, ref opeRES, ref output);
                                            opeRES = temp.Length / 2; //store the length of the operand in bytes
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nInvalid Operand!");
                                            error = true;
                                            return;
                                        } //else
                                    } //if is hex
                                } //else 
                                isExists(symbolTable, LMO[0], ref loc, ref lineCount, ref sourceCode, ref error);
                            } //if is C or X
                            else
                            {
                                Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nThe operand for BYTE must be either \'C\' or \'X\'.");
                                error = true;
                                return;
                            } //else
                        } //if mnem is BYTE
                        output.Add(loc.ToString("X").PadLeft(4, '0') + "\t" + LMO[0] + "\t" + LMO[1] + "\t" + LMO[2] + "\t" + "***" + "\t" + addr + "\t" + $"{lineCount + 1}"); //write the location
                    } //if
                    else if (LMO[1].Contains("\t") || string.IsNullOrWhiteSpace(LMO[1])) //if LMO[1] is null, empty or tab
                    {
                        if (opcodeTable.ContainsKey(LMO[0])) //if opcode is LMO[0] => this sourceCode without label
                        {
                            if (LMO[0] == "RSUB")
                            {
                                if (!string.IsNullOrWhiteSpace(LMO[2])|| LMO[2] != "\t")
                                {
                                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nRSUB cannot have operand!");
                                    error = true;
                                    return;
                                }
                            }
                            isPesudo(ref loc, ref opeRES, ref output);
                            output.Add(loc.ToString("X").PadLeft(4, '0') + "\t" + "***" + "\t" + LMO[0] + "\t" + LMO[2] + "\t" + opcodeTable[LMO[0]] + "\t" + addr + "\t" + $"{lineCount + 1}");
                        }
                        else
                        {
                            Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nIllegal instruction.");
                            error = true;
                        } //else
                        return;
                    } // else if
                    else if (opcodeTable.ContainsKey(LMO[0])) //if opcode is LMO[0] => this sourceCode without label
                    {
                        if (LMO[2].Contains("X"))
                        {
                            addr = "indexed";
                            isPesudo(ref loc, ref opeRES, ref output);
                            operand = LMO[1] + LMO[2];
                            output.Add(loc.ToString("X").PadLeft(4, '0') + "\t" + "***" + "\t" + LMO[0] + "\t" + operand + "\t" + opcodeTable[LMO[0]] + "\t" + addr + "\t" + $"{lineCount + 1}"); //write the object code to a file
                            return;
                        } //if
                        else
                        {
                            isPesudo(ref loc, ref opeRES, ref output);
                            output.Add(loc.ToString("X").PadLeft(4, '0') + "\t" + "***" + "\t" + LMO[1] + "\t" + LMO[2] + "\t" + "\t" + addr + "\t" + $"{lineCount + 1}"); //write the location
                        }
                    } //else if
                    else
                    {
                        Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nInvalid OpCode!");
                        error = true;
                        return;
                    } //else
                } //if mnemonic code is not in the opcode table
                else
                {
                    if (LMO[2].Contains(",X")) //index addressing or not
                        addr = "indexed";

                    if (LMO[0] != "") //if label exists
                    {
                        if (LMO[1] == "RSUB" && !string.IsNullOrEmpty(LMO[2]))
                        {
                            Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nRSUB cannot have operand!");
                            error = true;
                            return;
                        }
                        isPesudo(ref loc, ref opeRES, ref output);
                        isExists(symbolTable, LMO[0], ref loc, ref lineCount, ref sourceCode, ref error);
                        if (error == false)
                            output.Add(loc.ToString("X").PadLeft(4, '0') + "\t" + LMO[0] + "\t" + LMO[1] + "\t" + LMO[2] + "\t" + opcodeTable[LMO[1]] + "\t" + addr + "\t" + $"{lineCount + 1}"); //write the location
                    } //if exists label on LMO[0]
                    else //if label is null
                    {
                        if (LMO[1] == "RSUB" && LMO[2] != "")
                        {
                            Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nRSUB cannot have operand!");
                            error = true;
                            return;
                        }
                        isPesudo(ref loc, ref opeRES, ref output);
                        output.Add(loc.ToString("X").PadLeft(4, '0') + "\t" + "***" + "\t" + LMO[1] + "\t" + LMO[2] + "\t" + opcodeTable[LMO[1]] + "\t" + addr + "\t" + $"{lineCount + 1}"); //write the location
                    } //else
                } //else
            } //if
        } //sep_in_3

        public static void sep_in_2(string[] LMO, Dictionary<string, string> opcodeTable, List<string> sourceCode, ref int lineCount, ref int loc, 
            Dictionary<string, string> symbolTable, ref int opeRES, string addr, List<string> output, ref bool error, ref bool locHex)
        {
            if (LMO[1].Contains("X") || LMO[1].Contains("x"))
            {
                int Xcount = 1;
                if (LMO[1] == "x")
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\n索引定址錯誤!");
                    error = true;
                    return;
                } //if is x

                if (LMO[1].Count(c => char.ToUpper(c) == 'X') > Xcount || LMO[1].Count(c => c ==',') > Xcount)
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\n索引定址錯誤!");
                    error = true;
                    return;
                } //if x count is more than 1
                else
                    LMO[1] = Regex.Replace(LMO[1], pattern, ",X");
            } //if

            LMO[1] = Regex.Replace(LMO[1], pattern, "").Trim(); //remove the comment
            if (isSamefor2(LMO, sourceCode, ref lineCount, ref error, opcodeTable) == false)
            {
                if (!opcodeTable.ContainsKey(LMO[0]))
                {
                    if (pseudo.Contains(LMO[0])) //if is pesudo code
                    {
                        if (LMO[0] == pseudo[0] && locCheck(LMO[1], ref locHex, ref loc) == true) //if the mnemonic is START
                            loc = int.Parse(LMO[1], System.Globalization.NumberStyles.HexNumber);
                        else if (LMO[0] == pseudo[1]) //if the mnemonic is END
                            isPesudo(ref loc, ref opeRES, ref output);
                        else if (LMO[0] == pseudo[2] || LMO[0] == pseudo[3] || LMO[0] == pseudo[5]) //if the mnem is RESW or RESB
                        {
                            if (isDecimal(LMO[1]) == true) //if the operand is decimal
                            {
                                isPesudo(ref loc, ref opeRES, ref output); //check if the previous line is pesudo code
                                opeRES = int.Parse(LMO[1]); //store the operand value
                            }
                            else
                            {
                                Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\n Operand must be decimal");
                                error = true;
                                return;
                            } //else
                        } //if mnem is RESW or RESB
                        else if (LMO[0] == pseudo[4]) //if the mnem is BYTE
                        {
                            Match match = Regex.Match(LMO[1], @"'([^']*)'"); //match the operand value
                            string temp = "";
                            if (match.Success)
                                temp = match.Groups[1].Value; //store the operand value
                            else
                            {
                                Console.WriteLine($"Line {lineCount + 1}: {sourceCode[lineCount]}\nFail to get the operand.");
                                error = true;
                                return;
                            }

                            if (string.IsNullOrEmpty(temp))
                            {
                                Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nInvalid Operand! BYTE operand cannot be empty or only white space.");
                                error = true;
                                return;
                            } //if                                

                            if (LMO[1].StartsWith("C") || LMO[1].StartsWith("X"))
                            {
                                if (LMO[1].StartsWith("C")) //if the operand starts with C
                                {
                                    isPesudo(ref loc, ref opeRES, ref output);
                                    opeRES = temp.Length; //store the length of the operand
                                }
                                else
                                {
                                    string hex = @"\A[0-9a-fA-F]+\Z"; //pattern for hex
                                    if (Regex.IsMatch(temp, hex) == true) //if the operand is hex
                                    {
                                        if (temp.Length % 2 == 0)
                                        {
                                            isPesudo(ref loc, ref opeRES, ref output);
                                            opeRES = temp.Length / 2; //store the length of the operand in bytes
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nInvalid Operand!");
                                            error = true;
                                            return;
                                        } //else
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nOperand must be hex!");
                                        error = true;
                                        return;
                                    } //else if X
                                } //else if X
                            }
                            else
                            {
                                Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nThe operand for BYTE must be either \'C\' or \'X\'.");
                                error = true;
                                return;
                            } //else
                        } //else if mnem is BYTE
                        output.Add(loc.ToString("X").PadLeft(4, '0') + "\t" + "***" + "\t" + LMO[0] + "\t" + LMO[1] + "\t" + "***" + "\t" + addr + "\t" + $"{lineCount + 1}"); //write the location 
                    } //if
                    else if (opcodeTable.ContainsKey(LMO[1]) || LMO[1] == "RSUB") //if the opcode is exists in LMO[1] -> only RSUB can do this
                    {
                        if (LMO[0].Contains("\t") || LMO[0] == " " || LMO[0] == "") //if LMO[0] is null
                        {
                            isPesudo(ref loc, ref opeRES, ref output);
                            output.Add(loc.ToString("X").PadLeft(4, '0') + "\t" + "***" + "\t" + LMO[1] + "\t" + "***" + "\t" + opcodeTable[LMO[1]] + "\t" + addr + "\t" + $"{lineCount + 1}"); //write the object code to a file
                        } //if
                        else //if label exists
                        {
                            isPesudo(ref loc, ref opeRES, ref output);
                            isExists(symbolTable, LMO[0], ref loc, ref lineCount, ref sourceCode, ref error);
                            output.Add(loc.ToString("X").PadLeft(4, '0') + "\t" + LMO[0] + "\t" + LMO[1] + "\t" + "***" + "\t" + opcodeTable[LMO[1]] + "\t" + addr + "\t" + $"{lineCount + 1}");
                        } //if 
                    } //if
                    else
                    {
                        Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nInvalid OpCode!");
                        error = true;
                        return;
                    } //else
                }
                else
                {
                    if (LMO[1].Contains(",X")) //index addressing or not
                        addr = "indexed";

                    if (LMO[0] == "RSUB") // if RSUB without label
                    {
                        if (LMO[1] != "")
                        {
                            Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nRSUB cannot have operand!");
                            error = true;
                            return;
                        }
                        isPesudo(ref loc, ref opeRES, ref output);
                    }
                    else //if opcode is exists
                        isPesudo(ref loc, ref opeRES, ref output);
                    output.Add(loc.ToString("X").PadLeft(4, '0') + "\t" + "***" + "\t" + LMO[0] + "\t" + LMO[1] + "\t" + opcodeTable[LMO[0]] + "\t" + addr + "\t" + $"{lineCount + 1}"); //write the location
                } //else
            }
            else
                return;
        } //sep_in_2

        public static void sep_in_1(string[] LMO, Dictionary<string, string> opcodeTable, List<string> sourceCode, ref int lineCount, ref int loc,
            Dictionary<string, string> symbolTable, ref int opeRES, string addr, List<string> output, ref bool error)
        {
            if (opcodeTable.ContainsKey(LMO[0]))
            {
                if (LMO[0] == "RSUB")
                {
                    isPesudo(ref loc, ref opeRES, ref output);
                    output.Add(loc.ToString("X").PadLeft(4, '0') + "\t" + "***" + "\t" + LMO[0] + "\t" + "***" + "\t" + opcodeTable[LMO[0]] + "\t" + addr + "\t" + $"{lineCount + 1}"); //write the location
                } //if
                else
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nOpcode must have operand!");
                    error = true;
                } //else
            }
            else
            {
                Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nInvalid OpCode!");
                error = true;
            } //else
        } //sep_in_1

        public static void isExists(Dictionary<string, string> symbolTable, string label, ref int loc, ref int lineCount, ref List<string> sourceCode, 
            ref bool error)
        {
            if (symbolTable.ContainsKey(label)) //if label is exists
            {
                Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nDuplicate Label!");
                error = true;
            }
            else
                symbolTable.Add(label, loc.ToString("X").PadLeft(4, '0'));
        } //isExists

        public static void isPesudo(ref int loc, ref int opeRES, ref List<string> output)
        {
            if (output[output.Count - 1].Contains(pseudo[0])) //if the previous line contains START
                return;
            else if (output[output.Count - 1].Contains(pseudo[2])) //if the previous line contains RESB
                loc = loc + opeRES;
            else if (output[output.Count - 1].Contains(pseudo[3])) //if the previous line contains RESW
                loc = loc + opeRES * 3; //add the operand value to the location
            else if (output[output.Count - 1].Contains(pseudo[4])) //if the previous line contains BYTE
                loc += opeRES; //add operand to the location (byte is calculate with length)
            else if (output[output.Count - 1].Contains(pseudo[5])) //if the previous line contains WORD
                loc += 3;
            else //if the previous line is not in pesudo code
                loc += 3;
        } //isPesudo

        public static bool isDecimal(string operand)
        {
            int num;
            return int.TryParse(operand, out num);
        } //isDecimal

        public static bool isSamefor3(string[] LMO, List<string> sourceCode, ref int lineCount, ref bool isError, Dictionary<string, string> opcodeTable)
        {
            if (!opcodeTable.ContainsKey(LMO[1]))
            {
                if (pseudo.Contains(LMO[0]))
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nPseudo code cannot be used as label!");
                    isError = true;
                    return isError;
                }
                else if (LMO[1].Contains("\t") || string.IsNullOrWhiteSpace(LMO[1])) //if LMO[1] is null, empty or tab
                {
                    if (opcodeTable.ContainsKey(LMO[0]) && opcodeTable.ContainsKey(LMO[2])) //if opcode is LMO[0] => this sourceCode without label
                    {
                        Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nMnem cannot be used as operand!");
                        return isError = true;
                    }
                    else
                        return isError = false;
                }
                else if (opcodeTable.ContainsKey(LMO[0]) && opcodeTable.ContainsKey(LMO[1])) //if opcode is LMO[0] => this sourceCode without label
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nMnem cannot be used as operand!");
                    return isError = true;
                }
                else
                    return isError = false;
            } //if
            else
            {
                if (pseudo.Contains(LMO[2]))
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nPseudo code cannot be operand!");
                    return isError = true;
                }
                else if (pseudo.Contains(LMO[0]))
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nPseudo code cannot be label!");
                    return isError = true;
                }
                else if (opcodeTable.ContainsKey(LMO[0]))
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nOpcode cannot be label!");
                    return isError = true;
                }
                else if (LMO[0] == LMO[2])
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nOperand cannot be the same as label in one line!");
                    return isError = true;
                }
                else if (opcodeTable.ContainsKey(LMO[2]))
                {
                    Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nOperand cannot be opcode code!");
                    return isError = true;
                }
                else
                    return isError = false;
            }    
        } //isSame

        public static bool isSamefor2(string[] LMO, List<string> sourceCode, ref int lineCount, ref bool isError, Dictionary<string, string> opcodeTable)
        {
            if (opcodeTable.ContainsKey(LMO[1]) && LMO[1] != "RSUB")
            {
                Console.WriteLine($"Line {lineCount + 1} in Pass 1: {sourceCode[lineCount]}\nOpcode cannot be the operand!");
                return isError = true;
            }
            else
                return isError = false;
        }

        public static bool locCheck(string ope, ref bool locHex, ref int loc)
        {
            locHex = int.TryParse(ope, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out loc);

            return locHex;
        } //locCheck

        public static void startEnd(string[] keyword, ref bool isStart, ref bool isEnd, ref bool dupStart, ref bool dupEnd, 
            List<string> sourceCode)
        {
            int i = 0;
            int startCount = 0;
            int endCount = 0;
            int lineCount = 0;

            while (lineCount < sourceCode.Count)
            {
                if (sourceCode[lineCount].StartsWith(".") || sourceCode[lineCount].StartsWith(" ") || sourceCode[lineCount] == "")
                {
                    lineCount++;
                    continue;
                }
                else
                {
                    foreach (string s in keyword)
                    {
                        string pattern = $@"\b{Regex.Escape(s)}\b"; //create a pattern
                        Regex regex = new Regex(pattern);

                        if (regex.IsMatch(sourceCode[lineCount]) && s == keyword[0])
                        {
                            isStart = true;
                            startCount++;
                            break;
                        } //if
                        else if (regex.IsMatch(sourceCode[lineCount]) && s == keyword[1])
                        {
                            isEnd = true;
                            endCount++;
                            break;
                        } //else if
                    } //foreach
                } //else
               
                if (isStart == true && startCount > 1)
                    dupStart = true;

                if (isEnd == true && endCount > 1)
                    dupEnd = true;

                if (isStart == false)
                    throw new ErrorDetectedException($"Line {lineCount + 1} Error: {sourceCode[lineCount]}\nProgram must start at \"START\".");
                if (dupStart == true)
                    throw new ErrorDetectedException($"Line {lineCount + 1} Error: {sourceCode[lineCount]}\nSTART appears repeatedly.");

                if (isEnd == true)
                {
                    for (i = lineCount + 1; i < sourceCode.Count; i++)
                    {
                        if (!sourceCode[i].StartsWith(".") && !sourceCode[i].StartsWith(" ") && sourceCode[i] != "" && sourceCode[i] != "\n")
                            throw new ErrorDetectedException($"Line {i + 1} Error: {sourceCode[i]}\nProgram must end at \"END\".");
                    }
                }

                if (dupEnd == true)
                    throw new ErrorDetectedException($"Line {lineCount + 1} Error: {sourceCode[lineCount]}\nEND appears repeatedly. Please check the source code again.");

            lineCount++;
            } //while

            if (isEnd == false)
                throw new ErrorDetectedException($"Line {lineCount + 1}: \"END\" not found.");
        } //startEnd
    } //passOne
}
