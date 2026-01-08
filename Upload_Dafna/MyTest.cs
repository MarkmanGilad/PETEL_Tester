
using NUnit.Framework;
using System;

namespace LinkedListProject
{

[TestFixture]
public class MyTest
{

    //public static string METHOD_NAME;
    //public static string FILE_NAME = "RecursionDigitsUtil";
        
        
    [Test]
    public void Test01()
    {
        Console.WriteLine("\n01  Your List is:\n");
        Node<int> list = new Node<int>(27);
        list = new Node<int>(6,list);
        list = new Node<int>(18,list);
        list = new Node<int>(12,list);
        ListUtilDafnaM.PrintList(list);
            bool flag1 = false;
        	try{
			    flag1 = ListUtil.IsSortedUpList(list);
		    } 
            catch (StackOverflowException e) {
                Assert.Fail("  שגיאה, לולאה אינסופית, בדקו את קידום הלולאה");
            }
            catch(NullReferenceException e) { 
                Assert.Fail("Null Pointer Exception - null גישה למצביע שערכו\n .בדקו את זקיף הלולאה (תנאי העצירה) שהגדרתם או האם הרשימה ריקה"); 
            }
            catch (OutOfMemoryException e) {
                Assert.Fail("Memory limit exceeded - חריגה ממגבלת הזיכרון.\n בדקו את קידום החזרתיות - לולאה אינסופית");
            } 
            catch (Exception e) {
                Assert.Fail("An error occurred: " + e.Message);
                    Console.WriteLine(e.StackTrace);
            }

		//METHOD_NAME = "IsSortedUpList";
		//if (CheckInsideMethod.checkRecursion())
		//	fail("בפעולה מס' 2 - אין  שימוש  ברקורסיה");
			
		if (flag1)
			Console.WriteLine("01  List is Sorted Up ");
		else
			Console.WriteLine("01  List is NOT Sorted Up ");
		bool flag2 = ListUtilDafnaM.IsSortedUp(list);

		if (flag1==flag2)
		      Assert.IsTrue(true);
		  else
		      Assert.Fail("\n the correct answer: "+ flag2);
    }
	
    [Test]
    public void Test02()
    {
        Console.WriteLine("\n01  Your List is:\n");
        Node<int> list = new Node<int>(27);
        ListUtilDafnaM.PrintList(list);
            bool flag1 = false;
        	try{
			    flag1 = ListUtil.IsSortedUpList(list);
		    } 
            catch (StackOverflowException e) {
                Assert.Fail("  שגיאה, לולאה אינסופית, בדקו את קידום הלולאה");
            }
            catch(NullReferenceException e) { 
                Assert.Fail("Null Pointer Exception - null גישה למצביע שערכו\n .בדקו את זקיף הלולאה (תנאי העצירה) שהגדרתם או האם הרשימה ריקה"); 
            }
            catch (OutOfMemoryException e) {
                Assert.Fail("Memory limit exceeded - חריגה ממגבלת הזיכרון.\n בדקו את קידום החזרתיות - לולאה אינסופית");
            } 
            catch (Exception e) {
                Assert.Fail("An error occurred: " + e.Message);
                    Console.WriteLine(e.StackTrace);
            }

		//METHOD_NAME = "IsSortedUpList";
		//if (CheckInsideMethod.checkRecursion())
		//	fail("בפעולה מס' 2 - אין  שימוש  ברקורסיה");
			
		//if (flag1)
		//	Console.WriteLine("02  List is Sorted Up ");
		//else
		//	Console.WriteLine("02  List is NOT Sorted Up ");
		bool flag2 = ListUtilDafnaM.IsSortedUp(list);

		if (flag1==flag2)
		      Assert.IsTrue(true);
		  else
		      Assert.Fail("\n the correct answer: "+ flag2);
    }
    
    	
    [Test]
    public void Test03()
    {
        Console.WriteLine("\n01  Your List is:\n");
        Node<int> list = null;
        ListUtilDafnaM.PrintList(list);
            bool flag1 = false;
        	try{
			    flag1 = ListUtil.IsSortedUpList(list);
		    } 
            catch (StackOverflowException e) {
                Assert.Fail("  שגיאה, לולאה אינסופית, בדקו את קידום הלולאה");
            }
            catch(NullReferenceException e) { 
                Assert.Fail("Null Pointer Exception - null גישה למצביע שערכו\n .בדקו את זקיף הלולאה (תנאי העצירה) שהגדרתם או האם הרשימה ריקה"); 
            }
            catch (OutOfMemoryException e) {
                Assert.Fail("Memory limit exceeded - חריגה ממגבלת הזיכרון.\n בדקו את קידום החזרתיות - לולאה אינסופית");
            } 
            catch (Exception e) {
                Assert.Fail("An error occurred: " + e.Message);
                    Console.WriteLine(e.StackTrace);
            }

		//METHOD_NAME = "IsSortedUpList";
		//if (CheckInsideMethod.checkRecursion())
		//	fail("בפעולה מס' 2 - אין  שימוש  ברקורסיה");
			
		//if (flag1)
		//	Console.WriteLine("02  List is Sorted Up ");
		//else
		//	Console.WriteLine("02  List is NOT Sorted Up ");
		bool flag2 = ListUtilDafnaM.IsSortedUp(list);

		if (flag1==flag2)
		      Assert.IsTrue(true);
		  else
		      Assert.Fail("\n the correct answer: "+ flag2);
    }
}
}
