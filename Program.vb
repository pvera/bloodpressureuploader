Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Globalization
Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome
Imports OpenQA.Selenium.Support.UI
Imports WebDriverManager
Imports WebDriverManager.Helpers
Imports Microsoft.Extensions.Configuration

' NOTE: This script assumes a standard Console Application context.
' Install NuGet Packages: Selenium.WebDriver, Selenium.WebDriver.ChromeDriver, WebDriverManager, Microsoft.Extensions.Configuration.Json

Module AutoSubmitter
    ' Configuration
    Dim LoginUrl As String
    Dim FormUrl As String
    Dim CsvPath As String

    Sub Main(args As String())
        ' If --test argument is provided, run tests instead of the main application
        If args.Length > 0 AndAlso args(0) = "--test" Then
            CsvParserTests.RunTests()
            Return
        End If

        ' Load configuration from appsettings.json
        Dim config As IConfigurationRoot = New ConfigurationBuilder() _
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) _
    .AddJsonFile("appsettings.json", optional:=False, reloadOnChange:=True) _
    .Build()

        LoginUrl = config("BP_LOGIN_URL")
        FormUrl = config("BP_FORM_URL")
        CsvPath = config("CSV_PATH")

        Console.WriteLine("Initializing Browser Automation...")

        ' 1. Read and Parse the CSV Data
        Dim records As List(Of BpRecord) = CsvParser.ParseCsv(CsvPath)
        Console.WriteLine($"Loaded {records.Count} records to process.")

        ' 2. Setup ChromeDriver with automatic version management
        Dim driverManager As New WebDriverManager.DriverManager()
        driverManager.SetUpDriver(New WebDriverManager.DriverConfigs.Impl.ChromeConfig(), VersionResolveStrategy.MatchingBrowser)
        Dim options As New ChromeOptions()
        ' options.AddArgument("--headless") ' Uncomment to run invisible

        Using driver As IWebDriver = New ChromeDriver(options)
            Try
                ' 3. Perform Login
                Console.WriteLine("Navigating to login page...")
                driver.Navigate().GoToUrl(LoginUrl)

                ' --- INTERACTIVE LOGIN BLOCK ---
                ' Since authentication can be complex (MFA, etc.), we pause here.
                ' The script waits for you to manually log in and reach the dashboard.
                Console.WriteLine("--> PLEASE LOG IN MANUALLY IN THE BROWSER <--")
                Console.WriteLine("Press ENTER in this console once you are logged in.")
                Console.ReadLine()
                ' -------------------------------

                ' 4. Iterate and Submit Data
                Dim wait As New WebDriverWait(driver, TimeSpan.FromSeconds(10))
                Dim counter As Integer = 0

                ' Note: Parallel processing is avoided here because Selenium WebDriver is not thread-safe 
                ' and we are sharing a single authenticated browser session.
                For Each record In records
                    Try
                        Console.WriteLine($"Submitting: {record.DateValue} {record.TimeValue} - BP: {record.Systolic}/{record.Diastolic}")

                        ' Navigate to the form entry page
                        driver.Navigate().GoToUrl(FormUrl)

                        ' --- STRATEGY: KEYWORD XPATH SELECTORS ---
                        ' Based on source.html, we target inputs by partial ID matches.

                        ' A. Wait until the Date field is present and visible
                        wait.Until(Function(d) d.FindElements(By.XPath("//input[contains(@id, 'dateTaken')]")).Count > 0)

                        ' 1. Date Field (ID contains 'dateTaken')
                        Dim dateInput = driver.FindElement(By.XPath("//input[contains(@id, 'dateTaken')]"))
                        dateInput.Click()
                        dateInput.Clear()
                        dateInput.SendKeys(record.DateValue)

                        ' 2. Time Field (ID contains 'timeTaken')
                        Dim timeInput = driver.FindElement(By.XPath("//input[contains(@id, 'timeTaken')]"))
                        timeInput.Click()
                        ' Use Ctrl+A + Delete instead of Clear() to handle pre-filled time mask
                        timeInput.SendKeys(Keys.Control + "a")
                        timeInput.SendKeys(Keys.Delete)
                        timeInput.SendKeys(record.TimeValue)

                        ' 3. Systolic Field (ID contains 'Systolic')
                        Dim sysInput = driver.FindElement(By.XPath("//input[contains(@id, 'Systolic')]"))
                        sysInput.Click()
                        sysInput.Clear()
                        sysInput.SendKeys(record.Systolic)

                        ' 4. Diastolic Field (ID contains 'Diastolic')
                        Dim diaInput = driver.FindElement(By.XPath("//input[contains(@id, 'Diastolic')]"))
                        diaInput.Click()
                        diaInput.Clear()
                        diaInput.SendKeys(record.Diastolic)

                        ' 5. Submit Button
                        ' Targeted by the text inside the span: <span ...>Submit</span>
                        Dim submitBtn = driver.FindElement(By.XPath("//button[.//span[text()='Submit']]"))
                        submitBtn.Click()

                        counter += 1
                        ' Wait for page transition or success message before continuing
                        System.Threading.Thread.Sleep(2000)

                    Catch ex As Exception
                        Console.WriteLine($"[ERROR] Failed to submit record for {record.DateValue}: {ex.Message}")
                    End Try
                Next

                Console.WriteLine($"Job Complete. Successfully submitted {counter} records.")

            Catch ex As Exception
                Console.WriteLine($"[CRITICAL ERROR] {ex.Message}")
            Finally
                ' Cleanup
                driver.Quit()
            End Try
        End Using

        Console.WriteLine("Press any key to exit...")
        Console.ReadKey()
    End Sub

End Module

Public Class CsvParser
    ''' <summary>
    ''' Parses the CSV file based on the 4-column structure:
    ''' Col 1: Systolic
    ''' Col 2: Diastolic
    ''' Col 3: Date
    ''' Col 4: Time (AM/PM format, passed directly)
    ''' </summary>
    Public Shared Function ParseCsv(filePath As String) As List(Of BpRecord)
        Dim output As New List(Of BpRecord)

        If Not File.Exists(filePath) Then
            Console.WriteLine("CSV File not found.")
            Return output
        End If

        Dim lines = File.ReadAllLines(filePath)

        ' Skip header row
        For i As Integer = 1 To lines.Length - 1
            Dim line = lines(i)
            Dim parts = Regex.Split(line, ",(?=(?:[^""]*""[^""]*"")*[^""]*$)")

            ' We expect at least 4 columns now
            If parts.Length >= 4 Then
                Dim rec As New BpRecord()

                ' Clean quotes and whitespace
                rec.Systolic = parts(0).Trim(""""c).Trim()
                rec.Diastolic = parts(1).Trim(""""c).Trim()
                rec.DateValue = parts(2).Trim(""""c).Trim()

                ' Time: Pass through exactly as is (AM/PM format)
                rec.TimeValue = parts(3).Trim(""""c).Trim()

                output.Add(rec)
            End If
        Next

        Return output
    End Function
End Class

Public Class BpRecord
    ' Storing as Strings to strictly follow "no formatting needed" instruction
    Public Property Systolic As String
    Public Property Diastolic As String
    Public Property DateValue As String
    Public Property TimeValue As String
End Class