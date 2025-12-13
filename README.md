<p align="center">
   <img src="images/Logo.png" alt="..." width="450" height="300">   
</p>


# ממשק לבדיקת קוד - PeTel Tester C#

**מכון ויצמן למדע | PeTel**

מדריך זה מסביר כיצד להשתמש בממשק בדיקת הקוד (Tester) בשפת C# המיועד עבור סביבת **PeTel** (המבוססת על Moodle VPL). הממשק מאפשר למורים ליצור מטלות תכנות, להריץ בדיקות פונקציונליות (Case Tests) ובדיקות מבניות (Code Tests) על פתרונות תלמידים בצורה אוטומטית.

## תוכן עניינים
1. [כללי](#כללי)
2. [הכנת סביבת הפיתוח](#הכנת-סביבת-הפיתוח)
3. [הורדת והתקנת הממשק](#הורדת-והתקנת-הממשק)
4. [יצירת משימה חדשה](#יצירת-משימה-חדשה)
5. [בניית Tester - קבצי בסיס](#בניית-tester---קבצי-בסיס)
6. [עבודה עם Unit4Helper](#עבודה-עם-unit4helper)
7. [בניית בדיקות פונקציונליות (Case Tester)](#בניית-בדיקות-פונקציונליות-case-tester)
8. [בניית בדיקות מבנה ותחביר (Code Tester)](#בניית-בדיקות-מבנה-ותחביר-code-tester)
9. [העלאת הבדיקה ל-PeTel VPL](#העלאת-הבדיקה-ל-petel-vpl)

---

## כללי

ממשק בדיקת הקוד מורכב משלושה חלקים עיקריים:
1. **בדיקה פונקציונאלית**-**Case Tester**: בדיקה של תוצאות הרצת קוד התלמיד (Output/Return Value) בהשוואה לפתרון המורה.
2. **בדיקה תחבירית**-**Code Tester**: בדיקה של תחביר ומבנה הקוד (האם הפונקציה רקורסיבית? האם היא סטטית? האם יש לולאות מקוננות?).
3. **בדיקת בינה מלאכותית**-**AI Tester**: (בפיתוח) בדיקת קוד באמצעות מודל בינה מלאכותית.

תהליך העבודה כולל פיתוח הבדיקות בסביבת **Visual Studio**, הרצת בדיקות מקומיות על פתרונות אפשריים, ולבסוף העלאת הקבצים לסביבת PeTel.

---
---
## הכנת סביבת הפיתוח

סביבת הפיתוח הנדרשת היא **Visual Studio** (גרסת Community החינמית מספיקה).

### שלבי התקנה ודרישות:
1. בעת התקנת Visual Studio, יש לוודא שה-Workload בשם **NET desktop development.** מסומן.
2. יש לוודא שרכיב **NET Framework 4.7.2.** מותקן (אם חסר, יש להתקינו דרך ה-Installer בלשונית Individual Components).

<p align="center">
   <img src="images/vs_installer_workloads.png" alt="..." width="450" height="300">
</p>
---

## הורדת והתקנת הממשק

הממשק זמין כפרויקט פתוח ב-GitHub. ניתן להוריד אותו באחת משתי דרכים:

1. **Cloning**: שכפול הפרויקט באמצעות Git.
2. **Download ZIP**: הורדת קובץ ZIP ופתיחתו במחשב.

**כתובת המאגר:** `https://github.com/MarkmanGilad/PETEL_Tester`

### אפשרות 1: Cloning דרך Visual Studio
במסך הפתיחה בחר ב-**Clone a repository**, הזן את הכתובת הנ"ל ולחץ על **Clone**.
<p align="center">
<img src="images/vs_clone_repo.png" alt="..." width="450" height="300">
</p>

### אפשרות 2: הורדת ZIP
באתר GitHub, לחץ על כפתור **Code** ובחר **Download ZIP**. לאחר ההורדה, חלץ את הקבצים (Extract) ופתח את הקובץ `PETEL_Tester.slnx`.

<p align="center">
<img src="images/zip_download_repo.png" alt="..." width="450" height="300">
</p>
---

## יצירת משימה חדשה

כדי ליצור בדיקה למטלה חדשה, אנו יוצרים פרויקט חדש בתוך ה-Solution הקיים.

### שלב 1: יצירת פרויקט
1. בחלון Solution Explorer, לחץ קליק ימני על ה-Solution.
2. בחר **Add -> New Project**.
3. בחר בסוג פרויקט: **Console App (.NET Framework)** בשפת **C#**.
4. תן שם לפרויקט (לדוגמה: `CopyQueue`) וודא שה-Framework הוא **4.7.2**.

<p align="center">
   <img src="images\new_project_console1.png" alt="..." height="200" style="vertical-align: top; margin-right: 8px;">
   <img src="images\new_project_console2.png" alt="..." height="200" style="vertical-align: top; margin-right: 8px;">   
   <img src="images\new_project_console3.png" alt="..." height="200" style="vertical-align: top; margin-right: 8px;">   
</p>


### שלב 2: התקנת ספריות (NuGet)
1. לחץ קליק ימני על ה-Solution ובחר **Manage NuGet Packages for Solution**.
2. עבור ללשונית **Installed**.
3. אתר את הספרייה **Microsoft.CodeAnalysis.CSharp**.
4. סמן את הפרויקט החדש שיצרת.
5. **חשוב מאוד:** וודא שהגרסה המותקנת היא **4.10.0**. **אין לעדכן לגרסה חדשה יותר**. לחץ על Install.

<p align="center">
   <img src="images\nuget_manager1.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">
   <img src="images\nuget_manager2.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">   
</p>


### שלב 3: העתקת קבצי המערכת
יש להעתיק לפרויקט החדש את קבצי הליבה של הטסטר מתוך התיקייה `PETEL_Tester` המקורית.
1. קליק ימני על הפרויקט החדש -> **Add -> Existing Item**.
2. נווט לתיקיית `PETEL_Tester/PETEL_Tester`.
3. סמן והוסף את כל הקבצים **למעט** `Program.cs`. הקבצים הנדרשים כוללים את:
    * `CodeAnalyzer.cs`
    * `MainTester.cs`
    * `ObjectCloning.cs`
    * `ObjectComparer.cs`
    * `StudentAnswer.cs`
    * `TeacherAnswer.cs`
    * `Unit4.cs`
    * `VPLTester.cs`

<p align="center">
   <img src="images\add_existing_items1.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">
   <img src="images\add_existing_items2.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">   
</p>


### שלב 4: הגדרת פרויקט ונקודת כניסה (Entry Point)
1. קבע את הפרויקט שאותו ברצונך להריץ (startup project)
2. קליק ימני על הפרויקט החדש -> **Properties**.
3. שנה את ה-**Startup Object** ל-`PETEL_VPL.MainTester`.
4. שמור את השינויים (Ctrl+S).

<p align="center">
   <img src="images\startPoint1.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">
   <img src="images\startPoint2.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">   
</p>
<br>

כעת הפרויקט מוכן לפיתוח המשימה. ניתן להריץ (Ctrl+F5) כדי לראות פלט ברירת מחדל.


<p align="center">
   <img src="images\test_output1.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">
</p>

---
---

## בניית Tester - קבצי בסיס

תהליך בניית הבדיקה דורש עריכה של שלושה קבצים מרכזיים בפרויקט שיצרנו:

1. **TeacherAnswer.cs**: קובץ המכיל את הפתרון הנכון (של המורה). משמש כבסיס להשוואה ("האמת").
2. **StudentAnswer.cs**: קובץ המדמה פתרון של תלמיד. נשתמש בו כדי לבדוק שהטסטר שלנו מזהה שגיאות נכון (למשל, נכתוב בו פתרון שגוי בכוונה).
3. **MainTester.cs**: קובץ המנהל את הבדיקות (יוסבר בהרחבה בהמשך).

יש לכתוב את פתרון המורה והתלמיד. פתרון התלמיד יכול לכלול שגיאות לצורך בדיקת ה Tetser. 

ניקח לדוגמה את השאלה בה נדרש התלמיד לבנות פעולה המעתיקה תור מבלי לפגוע בתור המקורי. תשובת התלמיד שגויה בכך שהיא משנה את התור המקורי.

<p align="center">
   <img src="images\teacher_code.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">
   <img src="images\student_code.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">   
</p>

---

## עבודה עם Unit4Helper

הממשק כולל מחלקה בשם `Unit4Helper` המאפשרת יצירה והדפסה קלה של מבני נתונים (רשימות, תורים, מחסניות ועצים) לצורך כתיבת הבדיקות.

### פעולות עבור רשימת חוליות
```csharp
public static void NodeLIst()
{
    Node<int> lst = Unit4Helper.BuildNodeList(new int[] { 4, -2, 7, 0, -1, 0, 0 });
    Unit4Helper.PrintList(lst);
    int[] arr = Unit4Helper.NodeListToArray(lst);
    string str = Unit4Helper.NodeListToString(lst);
    Console.WriteLine(str);
}
```

### פעולות של תור
```csharp
public static void QueueMethods()
{
    Queue<int> q = Unit4Helper.BuildQueue(new int[] { 4, -2, 7, 0, -1, 0, 0 });
    Console.WriteLine(q);
    int[] arr = Unit4Helper.QueueToArray(q);
}
```

### פעולות של מחסנית
```csharp
public static void StackMethods() 
{
    Stack<int> s = Unit4Helper.BuildStack(new int[] { 4, -2, 7, 0, -1, 0, 0 });
    Console.WriteLine(s);
    int[] arr = Unit4Helper.StackToArray(s);
}
```



### עצים בינאריים
**עצים בינאריים:** ניתן לטעון עץ מקובץ טקסט המייצג את המבנה בהזחות (Tabs)


  ```csharp
public static void BinTreeMethods()
{
    string path = Unit4Helper.GetTreeFilePath("tree.txt");
    BinNode<int> tree = Unit4Helper.BuildBinaryTree<int>(path);
    Unit4Helper.PrintBinaryTree(tree);
    Unit4Helper.PrintBinaryTreeColored(tree);
    string str = Unit4Helper.BinaryTreeToString(tree);
    Console.WriteLine(str);
}
  ```

<p align="center">
   <img src="images\tree_txt.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">
   <img src="images\tree_print1.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">
   <img src="images\tree_print2.png" alt="..." height="250" style="vertical-align: top; margin-right: 8px;">
</p>

---

## בניית בדיקות פונקציונליות (Case Tester)

הקובץ `MainTester.cs` הוא הלב של מערכת הבדיקות. עלינו להגדיר את הפונקציה `CaseTester` שמבצעת את הבדיקות בפועל.

### אתחול (Main)
בפונקציה `Main`, אנו יוצרים מופע של `VPLTester` ומגדירים את שמות הקבצים והמחלקות:

```csharp
class MainTester
{
    public static void Main(string[] args)
    {
        // Initialize the tester
        var tester = new VPLTester(
            studentFile: "StudentAnswer.cs",
            studentNamespace: "",
            studentClassName: "StudentAnswer",
            studentMethodName: "Copy",
            teacherNamespace: "PETEL_VPL",
            teacherClassName: "TeacherAnswer",
            teacherMethodName: "Copy",
            showDetails: true
        );

        // Run all test suites
        CaseTester(tester);
        CodeTester(tester);

        // Display results (VPL parses this output)
        Console.WriteLine("\n" + tester.FormatResponse());
        Console.WriteLine($"Grade :=>> {tester.GetGrade()}");
    }
```

### הגדרת בדיקה (TestMethod)
הפונקציה `tester.TestMethod` מגדירה מקרה בדיקה בודד. להלן הפרמטרים העיקריים:

| פרמטר | סוג | תיאור |
| :--- | :--- | :--- |
| **testName** | `string` | שם הבדיקה והסבר קצר שיוצג לתלמיד. |
| **points** | `int` | ניקוד שיינתן במידה והבדיקה עברה. |
| **parameters** | `object[]` | מערך הפרמטרים שיישלחו לפונקציה הנבדקת. |
| **compareParams** | `bool` | האם לבדוק שינויים בפרמטרים המקוריים (Side Effects)? (ברירת מחדל: `true`). |
| **compareReturn** | `bool` | האם לבדוק את ערך ההחזרה? (ברירת מחדל: `true`). |
| **consoleInput** | `object` | קלט למסוף (אם הפונקציה מבקשת קלט מהמשתמש). |
| **captureConsoleOutput** | `bool` | האם להשוות את הפלט שהודפס למסוף (Console.WriteLine)? |
| **exceptionComments** | `Dictionary` | מילון הממיר שגיאות (Exceptions) להודעות מותאמות אישית. |

#### דוגמה לתרגיל:

כתוב פעולה המקבלת מספר שלם num. הפעולה תקלוט מחירים של מוצרים כמספר ה num. הפעולה תדפיס את סכום המוצרים ותחזיר את הממוצע שלהם.

**בדיקות**
```csharp
private static void CaseTester(VPLTester tester)
{
    tester.TestMethod(
        testName: "Test 1: 3 numbers. capture Console Output",
        points: 10,
        parameters: new object[] { 3 },
        compareParams: false,
        consoleInput: new string[] { "5", "3", "7" },
        captureConsoleOutput: true
    );

    tester.TestMethod(
        testName: "Test 2: 3 numbers. Only Compare Return",
        points: 10,
        parameters: new object[] { 3 },
        compareParams: false,
        consoleInput: new string[] { "5", "3", "4" },
        captureConsoleOutput: false
    );

    tester.TestMethod(
        testName: "Test 3: 1 numbers",
        points: 10,
        parameters: new object[] { 1 },
        compareParams: false,
        consoleInput: "3",
        captureConsoleOutput: true,
        compareReturn: false
    );
}
```

<p align="center">
   <img src="images\test_output1.png" alt="..." height="350" style="vertical-align: top; margin-right: 8px;">
</p>

---

#### דוגמה לתרגיל נוסף:
כתבו פעולה המעתיקה תור מבלי לפגוע בתור המקורי.
* בדיקה ראשונה – בודק רק את התור המוחזר ולא בודק אם התור המקורי השתנה.
* בדיקה שניה – בודק רק אם התור המקורי השתנה.
* בדיקה שלישית – בודק את שניהם
```csharp
private static void CaseTester(VPLTester tester)
{
    Queue<int> q1 = Unit4Helper.BuildQueue(new int[] { 3, 5, -9, 3, 5, 5, 2, 1, 2 });
    tester.TestMethod(
        testName: "Test 1: check the correct return. Don't check the original Queue ",
        points: 10,
        parameters: new object[] { q1 },
        compareParams: false
    );


    Queue<int> q2 = Unit4Helper.BuildQueue(new int[] { 3, 5, -9, 3, 5, 5, 2, 1, 2 });
    tester.TestMethod(
        testName: "Test 2: check only the original Queue if it changed",
        points: 10,
        parameters: new object[] { q2 },
        compareParams: true, 
        compareReturn:false
    );

    q2 = Unit4Helper.BuildQueue(new int[] { 3, 5, -9, 3, 5, 5, 2, 1, 2 });
    tester.TestMethod(
        testName: "Test 2: check both return and original",
        points: 10,
        parameters: new object[] { q2 },
        compareParams: true,
        compareReturn: true
    );
}

```

### הוספת הודעת שגיאה מותאמת אישית

באפשרותנו להוסיף להודעות השגיאה של המערכת הודעות שגיאה מותאמת אישית.

```csharp
 private static void CaseTester(VPLTester tester)
 {

     // Optional: Custom exception message
     var commonExceptionComments = new C.Dictionary<Type, string>
     {
         { typeof(NullReferenceException), "You tried to remove from empty queue" },
     };

     Queue<int> q1 = Unit4Helper.BuildQueue(new int[] { 3, 5, -9, 3, 5, 5, 2, 1, 2 });
     tester.TestMethod(
         testName: "Test 1: check the correct return. Don't check the original Queue ",
         points: 10,
         parameters: new object[] { q1 },
         compareParams: false,
         exceptionComments: commonExceptionComments
     );
```

* לדוגמה הודעת השגיאה המקורית:
<p align="center">
   <img src="images\Exception1.png" alt="..." height="350" style="vertical-align: top; margin-right: 8px;">
</p>

* לאחר השינוי הוספה ההודעה הבאה:
<p align="center">
   <img src="images\Exception2.png" alt="..." height="350" style="vertical-align: top; margin-right: 8px;">
</p>

---
---

## בניית בדיקות מבנה ותחביר (Code Tester)

במערכת PeTel ניתן לבדוק דרישות פדגוגיות ומבניות בקוד התלמיד באמצעות `CodeAnalyzer`. הבדיקות מתבצעות בפונקציה `CodeTester` ב-`MainTester.cs`.

ראשית, יש לאתחל את המנתח ולאחר מכן משתמשים בפונקציה `TestCodeStructure`. 
* נוסיף את שם הבדיקה והסבר.
* מספר הנקודות.
* סוג הבדיקה לפי רשימה קבועה מראש (ראו במהשך).
* פרמטרים נוספים אופציונאליים: כגון הודעת שגיאה.

**לדוגמה:**
```csharp
 private static void CodeTester(VPLTester tester)
{

    // Initialize code analyzer (handles errors internally)
    tester.InitializeCodeAnalyzer();

    // NEW: Check student method parameter list matches teacher method
    tester.TestCodeStructure(
        testName: "Test Params: method signature matches teacher",
        points: 5,
        checkType: CodeStructureCheck.CheckParams,
        shouldPass: true,
        failureMessage: "Wrong parameter list. Ensure the method has the same parameters as the teacher (name, count, and types)."
    );

    // NEW: Check return type matches teacher method
    tester.TestCodeStructure(
        testName: "Test Return Type: matches teacher",
        points: 5,
        checkType: CodeStructureCheck.CheckReturnType,
        shouldPass: true,
        failureMessage: "Wrong return type. Ensure the method returns the same type as the teacher."
    );

    // Test 11: Check that the method uses exactly one loop (O(n) complexity)
    tester.TestCodeStructure(
        testName: "Test 11: Uses exactly one loop",
        points: 10,
        checkType: CodeStructureCheck.CountAnyLoop,
        expectedCount: 1,
        failureMessage: "Method must use exactly one loop for O(n) time complexity"
    );
```

---
### פרמטרים של TestCodeStructure:


| פרמטר | סוג (Type) | פירוט והסבר | חובה/אופציונלי |
| :--- | :--- | :--- | :--- |
| **testName** | `String` | שם הבדיקה והסבר קצר שלה. הטקסט יופיע לתלמיד. | חובה |
| **points** | `Int` | מספר הנקודות שיקבל התלמיד אם יעבור את הבדיקה. | חובה |
| **checkType** | `CodeStructureCheck` | סוג הבדיקה לפי רשימה קבועה מראש (כגון `IsRecursive`, `HasNestedLoops` וכו'). | חובה |
| **shouldPass** | `Bool` | האם תוצאת הבדיקה המצופה היא חיובית או שלילית (ברירת מחדל: `true`). | אופציונלי |
| **expectedCount** | `Int?` | בבדיקות כמותיות: המספר המצופה (לדוגמה: מספר הלולאות). ברירת מחדל: `null`. | אופציונלי |
| **failureMessage** | `String` | הודעת שגיאה שתוצג לתלמיד אם הבדיקה נכשלת. ברירת מחדל: `null`. | אופציונלי |

---

### סוגי בדיקות (CodeStructureCheck)
להלן רשימה של הבדיקות האפשריות:

| שם הבדיקה (Check Name) | ערך מוחזר (Return Type) | תיאור הבדיקה |
| :--- | :--- | :--- |
| **IsRecursive** | `bool` | האם קיימת קריאה רקורסיבית בפעולה. |
| **CountForLoop** | `Int` | מספר לולאות `for` (לאו דווקא מקוננות) בהן השתמש התלמיד. |
| **CountWhileLoop** | `Int` | מספר לולאות `while` (לאו דווקא מקוננות) בהן השתמש התלמיד. |
| **CountForEachLoop** | `Int` | מספר לולאות `forEach` (לאו דווקא מקוננות) בהן השתמש התלמיד. |
| **CountAnyLoop** | `Int` | מספר לולאות מכל סוג בהן השתמש התלמיד (לאו דווקא מקוננות). |
| **CountIfStatements** | `Int` | מספר משפטי התנאי (`if`) בהם השתמש התלמיד. |
| **CountRecursiveCalls** | `Int` | מספר הקריאות הרקורסיביות בפעולה. |
| **CountReturnStatements** | `Int` | מספר פקודות ה-`return` בפעולה. |
| **CountNewNodes** | `Int` | מספר הפקודות `new Node<T>` בפעולה. |
| **CountNewQueue** | `Int` | מספר הפקודות `new Queue<T>` בפעולה. |
| **CountNewStack** | `Int` | מספר הפקודות `new Stack<T>` בפעולה. |
| **CountNewBinNode** | `Int` | מספר הפקודות `new BinNode<T>` בפעולה. |
| **CountSetNext** | `Int` | מספר פקודות `SetNext` של המחלקה `Node<T>`. |
| **CountGetNext** | `Int` | מספר פקודות `GetNext` של המחלקה `Node<T>`. |
| **HasNestedLoops** | `Bool` | האם התלמיד השתמש בלולאות מקוננות מכל סוג (משמש לבדיקת סיבוכיות). |
| **IsStatic** | `Bool` | בדיקת חתימת הפעולה (האם מוגדרת כ-Static). |
| **IsPublic** | `Bool` | בדיקת חתימת הפעולה (האם מוגדרת כ-Public). |
| **IsPrivate** | `Bool` | בדיקת חתימת הפעולה (האם מוגדרת כ-Private). |
| **IsProtected** | `Bool` | בדיקת חתימת הפעולה (האם מוגדרת כ-Protected). |
| **IsInternal** | `Bool` | בדיקת חתימת הפעולה (האם מוגדרת כ-Internal). |
| **CheckParams** | `Bool` | בדיקת חתימת הפעולה – השוואת הפרמטרים לחתימת פעולת המורה. |
| **CheckReturnType** | `Bool` | בדיקת חתימת הפעולה – השוואת הערך המוחזר לחתימת פעולת המורה. |

---


#### דוגמה לבדיקת יעילות (O(n)) ומניעת לולאות מקוננות:
```csharp
tester.TestCodeStructure(
    testName: "Test 11: No nested loops (O(n) complexity)",
    points: 10,
    checkType: CodeStructureCheck.HasNestedLoops,
    shouldPass: false, // אנו מצפים שהתוצאה תהיה False (אין לולאות מקוננות)
    failureMessage: "Method must not have nested loops"
);
```

---
<p align="center">
   <img src="images\PETEL.png" alt="..." height="350" style="vertical-align: top; margin-right: 8px;">
</p>
## העלאת הבדיקה ל-PeTel VPL

לאחר שהבדיקות עובדות מקומית ב-Visual Studio, יש להעלות את הקבצים לסביבת PeTel (Moodle).

### שלב 1: יצירת שאלה
1. בתוך Moodle/PeTel, צור פעילות חדשה מסוג **VPL Question**.
2. הגדר את שם הקובץ הנדרש מהתלמיד (למשל `StudentAnswer.cs`).
3. ב"תבנית תשובה" (Answer Template), הדבק את שלד הקוד של `StudentAnswer` כדי להקל על התלמיד.

### שלב 2: העלאת קבצי הבדיקה (Upload)
יש להעלות ל-VPL שני סוגי קבצים דרך ממשק **Test cases** -> **Upload**:

#### א. קבצי מערכת כלליים (Common Files)
קבצים אלו זהים לכל המשימות ונמצאים בתיקיית `Upload` בתיקייה שהורדתם מ-GitHub. יש להעלות את:
* `Tester.exe.b64` (הגרסה המקומפלת של מנוע הבדיקה)
* `vpl_evaluate.sh` (סקריפט הרצה)
* `vpl_run.sh`

#### ב. קבצי המשימה הספציפית
קבצים מתוך תיקיית הפרויקט שיצרתם (למשל `CopyQueue`):
* `MainTester.cs` (קובץ הבדיקות שכתבתם)
* `TeacherAnswer.cs` (פתרון המורה)

**שים לב:** אין להעלות את `StudentAnswer.cs` או קבצים אחרים שלא צוינו.

![Upload Files to VPL](images/vpl_upload_screen.png)

### שלב 3: אימות (Verification)
1. לחץ על שמירה.
2. עבור ללשונית "תצוגה מקדימה" (Preview).
3. הדבק פתרון תקין (או שגוי) בחלון העורך ולחץ על **בדיקה** (Check).
4. וודא שהפלט המתקבל זהה לפלט שראית ב-Visual Studio.

![PeTel Verification](images/petel_check_result.png)

---
**בהצלחה!**

