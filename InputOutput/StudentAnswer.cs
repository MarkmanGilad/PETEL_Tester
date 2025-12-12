using System;
using Unit4;

class StudentAnswer
{
    public static double InputOutput(int num)
    {
        double sum = 0;
        for (int i = 0; i < num; i++)
        {
            Console.WriteLine("Enter price");
            double price = double.Parse(Console.ReadLine());
            sum += price;
        }
        Console.WriteLine("The Sum is: " + sum);
        return sum / num;
    }
}

