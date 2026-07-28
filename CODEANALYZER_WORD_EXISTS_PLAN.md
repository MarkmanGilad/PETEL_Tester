# Plan: check whether a code word exists in a student method

## Goal

Add a CodeAnalyzer capability that lets a test author require or forbid a
specific C# code word in the method being tested. Examples include `new`,
`for`, and `while`.

The check must inspect the selected student method only, as the existing
structural checks do, rather than scanning the whole source file.

## Proposed behavior

- Match a complete C# lexical token, not a substring of source text.
- Treat C# keywords and punctuation/operators as tokens where applicable.
- Ignore text in comments and string/character literals.
- Do not treat an identifier containing the requested text as a match. For
  example, searching for `for` must not match `before` or `format`.
- Use ordinal, case-sensitive matching, consistent with C# keywords.
- Report both whether the token was found and how many occurrences were found
  for clear feedback and future count-based checks.

This means searching for `new` succeeds for an object-creation expression,
searching for `for` succeeds for a `for` statement, and searching for `while`
succeeds for a `while` statement. A quoted value such as `"while"` and a
comment containing `while` do not count.

## API design

1. In `PETEL_V2_Core/CodeAnalyzer.cs`, add a public method on `MethodAnalyzer`
   named `ContainsToken(string tokenText)`. It returns `true` when the method
   contains at least one exact token with that source text.
2. Add a companion count method, such as `CountTokens(string tokenText)`, so
   the boolean method can be implemented as a count greater than zero and
   teachers can later require an exact number of uses.
3. Add a corresponding convenience method on `CodeAnalyzer` that accepts the
   method name and token text. This keeps the feature usable directly from the
   analyzer without requiring callers to obtain a `MethodAnalyzer` themselves.
4. Use Roslyn's token APIs from the already-parsed `MethodDeclarationSyntax`.
   Do not use `ToString`, `Contains`, regular expressions, or a raw-text scan.

## How a teacher will use it in `TestCases.cs`

The new public tester method should be named `TestCodeTokenExists`. A teacher
will call it inside an existing `Code_...` test method:

```csharp
public static void Code_2_CheckRequiredWords(VPLTester tester)
{
    tester.TestCodeTokenExists(
        testName: "The solution creates a new object",
        points: 5,
        tokenText: "new",
        shouldExist: true,
        failureMessage: "Use the new keyword to create the required object."
    );

    tester.TestCodeTokenExists(
        testName: "The solution does not use a while loop",
        points: 5,
        tokenText: "while",
        shouldExist: false,
        failureMessage: "Do not use a while loop in this solution."
    );
}
```

`TestCodeTokenExists` checks the method named by `CreateTester()` (for example,
`CountValues`) in the student's source file. It gives the points when the
requested token is present for `shouldExist: true`, or absent for
`shouldExist: false`.

The method signature to add to `VPLTester` is:

```csharp
public void TestCodeTokenExists(
    string testName,
    int points,
    string tokenText,
    bool shouldExist = true,
    string? failureMessage = null)
```

## New function to implement: `TestCodeTokenExists`

Add this public function to `PETEL_V2_Core/VPLTester.cs`.

Its implementation will:

1. Confirm that `studentCodeAnalyzer` was initialized; otherwise produce the
   same controlled test failure used by `TestCodeStructure`.
2. Call `studentCodeAnalyzer.ContainsToken(StudentMethodName, tokenText)` to
   inspect the selected student method.
3. Treat the test as successful when the returned value equals `shouldExist`.
   In other words: `true` requires the token and `false` forbids it.
4. If the condition fails, create a `TestAssertionException` using
   `failureMessage` when supplied; otherwise use a default message that names
   the token and says whether it was required or forbidden.
5. Add points and format the result through the same grade/result path as
   `TestCodeStructure`, so teachers receive standard VPL feedback.

`TestCodeTokenExists` will not read source text itself. Its only code-analysis
operation is the Roslyn-backed `CodeAnalyzer.ContainsToken` call.

## Integration with the tester API

1. Add a `CodeStructureCheck.ContainsToken` value for the internal
   CodeAnalyzer dispatch, with an optional token-text argument. Validate that
   the argument is present and non-empty, and return a controlled descriptive
   failure if it is not.
2. Add `VPLTester.TestCodeTokenExists` as the teacher-facing API shown above.
   It forwards the requested token to CodeAnalyzer and preserves every
   existing `TestCodeStructure` call unchanged.
3. Use `shouldExist` so teachers can both require and forbid a word. Requiring
   `new` uses `shouldExist: true`; forbidding `while` uses
   `shouldExist: false`.
4. Format failures through the existing VPL result path, including the token
   and actual occurrence count in the description. No runner changes should be
   needed because `Code_*` tests already execute in `MainTesterHost`.

## Documentation and example updates

1. Add the new check to the `CodeStructureCheck` table in `README.md`.
2. Add one short `Code_...` example in `PETEL_MainTester_V2/TestCases.cs` (or
   an alternative example test file) demonstrating a required `new` token and
   a forbidden `while` token.
3. Update `summarization.md` to note that CodeAnalyzer has exact
   method-token checks in addition to its syntax-structure checks.

## Verification plan

Add focused tests or local sample cases covering:

| Scenario | Expected result |
| --- | --- |
| A method containing `new Node<int>()` searched for `new` | Found, count 1 |
| A method with one `for` and one `while` | Each searched token is found once |
| A method containing `before` and `format`, searched for `for` | Not found |
| A comment containing `while`, searched for `while` | Not found |
| A string literal containing `new`, searched for `new` | Not found |
| A forbidden `while` structural test when no `while` is present | Passes |
| A required token omitted from the selected method but present elsewhere in the file | Fails |

### Verify `TestCodeTokenExists`

Use a temporary/local `StudentAnswer.cs` implementation and add a `Code_...`
method in `PETEL_MainTester_V2/TestCases.cs` that calls
`TestCodeTokenExists`. Run `PETEL_MainTester_V2` for each case below and
confirm both the awarded points and the VPL feedback message.

| Student method content | Test call | Expected outcome |
| --- | --- | --- |
| Contains `new Node<int>(value)` | require `new` | Test passes and awards its points |
| Contains no object creation | require `new` | Test fails and shows the configured failure message |
| Contains no `while` statement | forbid `while` | Test passes and awards its points |
| Contains a `while` statement | forbid `while` | Test fails and shows the configured failure message |
| Has `// while` or `string s = "while"` only | forbid `while` | Test passes; comments and literals are ignored |
| Has `before` or `format` only | require `for` | Test fails; partial identifier matches are ignored |

Also rerun the existing `Code_1_MustUseRecursion` sample unchanged. It must
produce the same result as before, confirming that the new method did not alter
existing `TestCodeStructure` behavior.

Finally, build the V2 solution and run the local main-tester sample to confirm
that existing code-structure and functional tests remain unchanged.

## Files expected to change during implementation

- `PETEL_V2_Core/CodeAnalyzer.cs`
- `PETEL_V2_Core/VPLTester.cs`
- `PETEL_MainTester_V2/TestCases.cs` (example only)
- `README.md`
- `summarization.md`

`PETEL_Runner_V2` is not expected to require a change because this feature is a
source-only `Code_*` check executed by the main tester.
