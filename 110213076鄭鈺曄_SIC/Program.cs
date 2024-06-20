using System;
using System.IO;
using _110213076_Final;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection.Metadata.Ecma335;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Emit;

namespace SP_110213076_Final
{
    class Program
    {
        static void Main(string[] args)
        {
            #region global
            bool isError = false; //error flag
            bool isStart = false; //start flag
            bool isEnd = false; //end flag
            bool locHex = false; //location counter is hex or not
            bool dupStart = false; //duplicate start flag
            bool dupEnd = false; //duplicate end flag
            char[] delim = { '\t', ';', '\r', ' ' }; //Record delimiter
            int lineCount = 0;
            int loc = 0;
            int opeRES = 0; //store the operand value of RESB or RESW
            int Prog_Length = 0; //store the program length
            int i = 0;
            string addr = ""; //store the address
            string line;
            string startAddress; //store the start address
            string endAddress; //store the end address
            string startLabel; //store the start label
            string endLabel; //store the end label
            string[] keyword = new string[] {"START", "END"};
            string[] LMO = new string[3]; //Label, Mnemonic, Operand
            string[] sep_middle = new string[6];
            Dictionary<string, string> opcodeTable = new Dictionary<string, string>(); //store the opcode table
            Dictionary<string, string> symbolTable = new Dictionary<string, string>(); //store the symbol table
            List<string> sourceCode = new List<string>(); //store the source code
            List<string> output = new List<string>();
            List<string> inter = new List<string>();
            StreamReader middle;
            StreamReader fileReader;
            StreamWriter fileWriter = new StreamWriter("110213076鄭鈺曄_File.txt"); 
            StreamWriter symbol = new StreamWriter("110213076鄭鈺曄_symbol.txt", false);
            StreamWriter objectFile = new StreamWriter("110213076鄭鈺曄_objectFile.txt", false);
            #endregion global

            #region read the file
            op.readOp(opcodeTable);

            fileReader = File.OpenText("(test)SIC.txt"); //正確的原始檔
            while (!fileReader.EndOfStream) //record the code into a list
             {
                line = fileReader.ReadLine(); //record every line read in
                sourceCode.Add(line); //add the line to the list
                } //while
            fileReader.Close();
            #endregion read the file            

            #region Check if start and end are correct
            try 
            {
                passOne.startEnd(keyword, ref isStart, ref isEnd, ref dupStart, ref dupEnd, sourceCode);
            }
            catch (ErrorDetectedException ex) //catch the error
            {
                Console.WriteLine($"Error: {ex.Message}");
                return;
            }
            #endregion Check if start and end are correct

            #region passOne
            while (lineCount < sourceCode.Count)
            {
                addr = "direct"; //default addressing mode
                if (sourceCode[lineCount].StartsWith(".") || sourceCode[lineCount].StartsWith(" ") || sourceCode[lineCount] == "")
                {
                    lineCount++;
                    continue;
                }
                else
                {
                    LMO = sourceCode[lineCount].Split(delim, 3); //split the line into Label, Mnemonic, Operand

                    if (LMO.Count() == 1)
                        passOne.sep_in_1(LMO, opcodeTable, sourceCode, ref lineCount, ref loc, symbolTable, ref opeRES, addr, output, ref isError);
                    else if (LMO.Count() == 2)
                        passOne.sep_in_2(LMO, opcodeTable, sourceCode, ref lineCount, ref loc, symbolTable, ref opeRES, addr, output, ref isError, ref locHex);
                    else
                        passOne.sep_in_3(LMO, opcodeTable, sourceCode, ref lineCount, ref loc, symbolTable, ref opeRES, addr, output, ref isError, ref locHex);

                    if (locHex == false)
                    {
                        Console.ReadKey();
                        return;
                    }
                } //if
                lineCount++;
            } //while

            foreach (string s in output)
                fileWriter.WriteLine(s); //write the middle file
            foreach (KeyValuePair<string, string> kvp in symbolTable)
                symbol.WriteLine(kvp.Key + "\t" + kvp.Value); //write the symbol table

            fileWriter.Close(); //close the file writer
            symbol.Close(); //close the symbol file
            #endregion passOne

            #region passTwo
            middle = File.OpenText("110213076鄭鈺曄_File.txt");
            output.Clear(); //clear the output list
            lineCount = 0; //reset the line count

            while (!middle.EndOfStream)
            {
                line = middle.ReadLine(); //read the middle file
                inter.Add(line); //add the line to the source code list
            } //while

            middle.Close(); //close the middle file

            sep_middle = inter[0].Split('\t', 7); //split the first line into 6 parts [loc /label /mnem/ operand/ opcode Value/ addressing]
            startAddress = sep_middle[0]; //store the start address
            startLabel = sep_middle[1]; //store the start label

            sep_middle = inter[inter.Count - 1].Split('\t', 7);
            endAddress = sep_middle[0]; //store the end address
            endLabel = sep_middle[3]; //store the end label

            Prog_Length = int.Parse(endAddress, System.Globalization.NumberStyles.HexNumber) - int.Parse(startAddress, System.Globalization.NumberStyles.HexNumber); //store the program length

            lineCount = 1; //start from the second line

            passTwo.UndefinedOperand(symbolTable, inter, ref isError); //check if the operand is defined or not

            while (lineCount < inter.Count - 1 && isError == false)
            {
                sep_middle = inter[lineCount].Split('\t', 7); //split the line into 6 parts [loc /label /mnem/ operand/ opcode Value/ addressing]
                passTwo.GenerateCode(sep_middle[4], sep_middle[3], symbolTable, sep_middle[2], output, sep_middle[5]); //generate the object code
                lineCount++;
            }

            lineCount = 0;

            if (isError == false)
            {
                objectFile.WriteLine($"H^{startLabel}\t^{startAddress.PadLeft(6, '0')}^{Prog_Length.ToString("X").PadLeft(6, '0')}"); //write the header record
                passTwo.WriteFile(output, objectFile, inter, ref isError); //write the text record
                passTwo.mRecord(inter, objectFile); //write the modification record
                objectFile.WriteLine($"E^{symbolTable[endLabel].PadLeft(6, '0')}"); //write the end record
            } //if

            objectFile.Close(); //close the object file
            #endregion passTwo

            Console.ReadKey(); //pause the program
        } //Main
    } //Program
} //namespace

