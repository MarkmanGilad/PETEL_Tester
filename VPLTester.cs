public class VPLTester
{
    private int grade = 0;
    private List<string> testResults = new List<string>();
    private ObjectComparer comparer;
    private CodeAnalyzer studentCodeAnalyzer;
    
    // Convert to public properties with getters and setters
    public string StudentSourceFilePath { get; set; }
    public string StudentNamespace { get; set; }
    public string StudentClassName { get; set; }
    public string StudentMethodName { get; set; }
    public string TeacherNamespace { get; set; }
    public string TeacherClassName { get; set; }
    public string TeacherMethodName { get; set; }
    public int TimeoutMilliseconds { get; set; }
    public bool ShowDetails { get; set; }

    public VPLTester(string studentFile, string studentNamespace, string studentClassName,
                     string studentMethodName, string teacherNamespace, string teacherClassName,
                     string teacherMethodName, bool showDetails = false, int timeoutMilliseconds = 2000)
    {
        StudentSourceFilePath = GetStudentSourcePath(studentFile);
        StudentNamespace = studentNamespace;
        StudentClassName = studentClassName;
        StudentMethodName = studentMethodName;
        TeacherNamespace = teacherNamespace;
        TeacherClassName = teacherClassName;
        TeacherMethodName = teacherMethodName;
        ShowDetails = showDetails;
        TimeoutMilliseconds = timeoutMilliseconds;
        comparer = new ObjectComparer();
    }
    
    // Keep convenience methods for common operations
    public void SetStudentMethod(string methodName) 
        => StudentMethodName = methodName;
    
    public void SetTeacherMethod(string methodName) 
        => TeacherMethodName = methodName;
    
    // ... rest of the class
}