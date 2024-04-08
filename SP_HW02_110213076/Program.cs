using System;

namespace SP_HW02_110213076
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> symbolTable = new Dictionary<string, string>();
            string input;
            string search;
            int i = 0;

            Console.Write("Please input label and address: ");
            input = Console.ReadLine();
            string[] data = input.Split(' '); // Split the input by space
            for (i = 0; i < data.Length; i += 2)
                symbolTable.Add(data[i], data[i + 1]); // Add the label and address to the symbol table

            Console.Write("Search (input the label): ");
            search = Console.ReadLine();
            if (symbolTable.ContainsKey(search)) // Check if the label exists in the symbol table
                Console.WriteLine("Address: " + symbolTable[search]); // Print the address
            else
                Console.WriteLine("Invalid");

            Console.ReadKey();
        }
    }
}
