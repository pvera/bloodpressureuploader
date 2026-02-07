Imports System
Imports System.IO
Imports System.Collections.Generic

Module CsvParserTests

    Sub RunTests()
        Console.WriteLine("Running CsvParser Tests...")
        Console.WriteLine()

        Try
            TestParseValidData()
            Console.WriteLine("? ParseCsv_WithValidData - PASSED")
        Catch ex As Exception
            Console.WriteLine($"? ParseCsv_WithValidData - FAILED: {ex.Message}")
        End Try

        Try
            TestParseQuotedData()
            Console.WriteLine("? ParseCsv_WithQuotedData - PASSED")
        Catch ex As Exception
            Console.WriteLine($"? ParseCsv_WithQuotedData - FAILED: {ex.Message}")
        End Try

        Try
            TestParseNonExistentFile()
            Console.WriteLine("? ParseCsv_WithNonExistentFile - PASSED")
        Catch ex As Exception
            Console.WriteLine($"? ParseCsv_WithNonExistentFile - FAILED: {ex.Message}")
        End Try

        Try
            TestParseSkipsHeaderRow()
            Console.WriteLine("? ParseCsv_SkipsHeaderRow - PASSED")
        Catch ex As Exception
            Console.WriteLine($"? ParseCsv_SkipsHeaderRow - FAILED: {ex.Message}")
        End Try

        Console.WriteLine()
        Console.WriteLine("All tests completed!")
    End Sub

    Private Sub TestParseValidData()
        Dim testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests", "test_data_valid.csv")
        
        Dim records = CsvParser.ParseCsv(testFile)

        If records.Count <> 2 Then
            Throw New Exception($"Expected 2 records, got {records.Count}")
        End If

        If records(0).Systolic <> "120" Then
            Throw New Exception($"Expected Systolic '120', got '{records(0).Systolic}'")
        End If

        If records(0).Diastolic <> "80" Then
            Throw New Exception($"Expected Diastolic '80', got '{records(0).Diastolic}'")
        End If

        If records(0).DateValue <> "01/15/2024" Then
            Throw New Exception($"Expected DateValue '01/15/2024', got '{records(0).DateValue}'")
        End If

        If records(0).TimeValue <> "9:30 AM" Then
            Throw New Exception($"Expected TimeValue '9:30 AM', got '{records(0).TimeValue}'")
        End If

        If records(1).Systolic <> "130" Then
            Throw New Exception($"Expected Systolic '130', got '{records(1).Systolic}'")
        End If

        If records(1).Diastolic <> "85" Then
            Throw New Exception($"Expected Diastolic '85', got '{records(1).Diastolic}'")
        End If
    End Sub

    Private Sub TestParseQuotedData()
        Dim testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests", "test_data_quoted.csv")
        
        Dim records = CsvParser.ParseCsv(testFile)

        If records.Count <> 2 Then
            Throw New Exception($"Expected 2 records, got {records.Count}")
        End If

        If records(0).Systolic <> "120" Then
            Throw New Exception($"Expected Systolic '120', got '{records(0).Systolic}'")
        End If

        If records(0).Diastolic <> "80" Then
            Throw New Exception($"Expected Diastolic '80', got '{records(0).Diastolic}'")
        End If
    End Sub

    Private Sub TestParseNonExistentFile()
        Dim testFile = "C:\nonexistent\file.csv"

        Dim records = CsvParser.ParseCsv(testFile)

        If records.Count <> 0 Then
            Throw New Exception($"Expected 0 records for non-existent file, got {records.Count}")
        End If
    End Sub

    Private Sub TestParseSkipsHeaderRow()
        Dim testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests", "test_data_valid.csv")

        Dim records = CsvParser.ParseCsv(testFile)

        ' Check that header row is not in the results
        For Each record In records
            If record.Systolic = "Systolic" Then
                Throw New Exception("Header row was not skipped!")
            End If
        Next
    End Sub

End Module
