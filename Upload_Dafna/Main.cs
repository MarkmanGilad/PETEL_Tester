using System;
using System.Text;
using NUnit.Framework;
//using MyNTest;

namespace LinkedListProject
{

class TestStudentAnswers 
{
    public static void Main() 
    {
       MyTest t = new MyTest();
        int grade = 0;

        Console.WriteLine();
        
        try
        {
            t.Test01();
            Console.WriteLine(FormatOutput("Test 1", "4", null));
            grade += 4;
        }
        catch (AssertionException e)
        {
            Console.WriteLine(FormatOutput("Test 1", "4", e));
        }
        
                Console.WriteLine();
    
        try
        {
            t.Test02();
            Console.WriteLine(FormatOutput("Test 2", "3", null));
            grade += 3;
        }
        catch (AssertionException e)
        {
            Console.WriteLine(FormatOutput("Test 2", "3", e));
        }
        
                Console.WriteLine();
        try
        {
            t.Test03();
            Console.WriteLine(FormatOutput("Test 3", "3", null));
            grade += 3;
        }
        catch (AssertionException e)
        {
            Console.WriteLine(FormatOutput("Test 3", "3", e));
        }
        
       Console.WriteLine("Grade :=>> " + grade);
  /*          
        try
        {
            t.Test03();
            Console.WriteLine(FormatOutput("Test 3", "10", null));
            grade += 10;
        }
        catch (AssertionException e)
        {
            Console.WriteLine(FormatOutput("Test 3", "10", e));
        }
        
                Console.WriteLine();
        
        try
        {
            t.Test04();
            Console.WriteLine(FormatOutput("Test 4", "10", null));
            grade += 10;
        }
        catch (AssertionException e)
        {
            Console.WriteLine(FormatOutput("Test 4", "10", e));
        }
        
                Console.WriteLine();
        
        try
        {
            t.Test05();
            Console.WriteLine(FormatOutput("Test 5", "10", null));
            grade += 10;
        }
        catch (AssertionException e)
        {
            Console.WriteLine(FormatOutput("Test 5", "10", e));
        }
        
                Console.WriteLine();
        
        try
        {
            t.Test06();
            Console.WriteLine(FormatOutput("Test 6", "10", null));
            grade += 10;
        }
        catch (AssertionException e)
        {
            Console.WriteLine(FormatOutput("Test 6", "10", e));
        }
        
                Console.WriteLine();
        
        try
        {
            t.Test07();
            Console.WriteLine(FormatOutput("Test 7", "10", null));
            grade += 10;
        }
        catch (AssertionException e)
        {
            Console.WriteLine(FormatOutput("Test 7", "10", e));
        }
        
                Console.WriteLine();
        
        try
        {
            t.Test08();
            Console.WriteLine(FormatOutput("Test 8", "10", null));
            grade += 10;
        }
        catch (AssertionException e)
        {
            Console.WriteLine(FormatOutput("Test 8", "10", e));
        }
       
                      Console.WriteLine();
        
        try
        {
            t.Test09();
            Console.WriteLine(FormatOutput("Test 9", "10", null));
            grade += 10;
        }
        catch (AssertionException e)
        {
            Console.WriteLine(FormatOutput("Test 9", "10", e));
        }
        
                       Console.WriteLine();
        
        try
        {
            t.Test10();
            Console.WriteLine(FormatOutput("Test 10", "10", null));
            grade += 10;
        }
        catch (AssertionException e)
        {
            Console.WriteLine(FormatOutput("Test 10", "10", e));
        }
      */  
        
        Console.WriteLine("Grade :=>> " + grade);

/*
        int result = (int)((float)grade + 5) / 10;
        switch (result)
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
                Console.WriteLine("      בפעם הבאה נצליח יותר");
                break;
            case 8:
                Console.WriteLine("      !טוב");
                break;
            case 9:
                Console.WriteLine("      !טוב מאד");
                break;
            case 10:
                Console.WriteLine("       !מצויין");
                break;
            default:
                throw new ArgumentException("Unexpected value: " + ((float)grade + 5) / 10);
        }
  */       
    }
        

    private static string FormatOutput(string testName, string value, AssertionException e)
        {
            var sb = new StringBuilder();
            var grade = (e == null ? value : "0");
            sb.AppendLine($"Comment :=>>{testName}: {(e == null ? "success" : "failure")}. {grade} points");
            if (e != null)
            {
                sb.AppendLine("<|--");
                sb.AppendLine(e.Message);
                sb.AppendLine("--|>");
            }
            return sb.ToString();
        }
}
}