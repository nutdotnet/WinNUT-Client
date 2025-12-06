Imports WinNUT_Client_Common

Public Class List_Var_Gui
    Private List_Var_Datas As List(Of UPS_List_Datas)
    Private UPSDevice As UPS_Device
    Private UPS_Name = WinNUT.UPS_Device.Nut_Config.UPSName

    Public Sub New(upsDev As UPS_Device)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        UPSDevice = upsDev
    End Sub

    Private Sub List_Var_Gui_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LogFile.LogTracing("Load List Var Gui", LogLvl.LOG_DEBUG, Me)
        Icon = WinNUT.Icon
        Visible = False
        PopulateTreeView()
        Visible = True
    End Sub

    Private Sub PopulateTreeView()
        LogFile.LogTracing("Populate TreeView", LogLvl.LOG_DEBUG, Me)
        Dim action As Action

        Try
            UPSDevice.IsUpdatingData = False
            List_Var_Datas = WinNUT.UPS_Device.GetUPS_ListVar()
            UPSDevice.IsUpdatingData = True
        Catch ex As Exception
            ' TODO: Internationalize?
            MessageBox.Show("Error encountered trying to get variables from the UPS: " & vbNewLine & ex.Message, "Error Encountered")
            ' Close()
            Return
        End Try

        If List_Var_Datas Is Nothing Then
            LogFile.LogTracing("ListUPSVars return Nothing Value", LogLvl.LOG_DEBUG, Me)
            Return
        End If

        action = Sub() TView_UPSVar.Nodes.Clear()
        TView_UPSVar.Invoke(action)
        action = Sub() TView_UPSVar.Nodes.Add(My.Settings.NUT_UPSName, My.Settings.NUT_UPSName)
        TView_UPSVar.Invoke(action)
        Dim TreeChild As New TreeNode
        Dim LastNode As New TreeNode
        For Each UPS_Var In List_Var_Datas
            LastNode = TView_UPSVar.Nodes(0)
            Dim FullPathNode = String.Empty
            For Each SubPath In (Split(UPS_Var.VarKey, "."))
                FullPathNode += SubPath & "."
                Dim Nodes = TView_UPSVar.Nodes.Find(FullPathNode, True)
                If Nodes.Length = 0 Then
                    If LastNode.Text = "" Then
                        action = Sub() LastNode = TView_UPSVar.Nodes.Add(FullPathNode, SubPath)
                        TView_UPSVar.Invoke(action)
                    Else
                        action = Sub() LastNode = LastNode.Nodes.Add(FullPathNode, SubPath)
                        TView_UPSVar.Invoke(action)
                    End If
                Else
                    LastNode = Nodes(0)
                End If
            Next
        Next
    End Sub

    Private Function FindNodeByValue(ByVal value As String, ByVal nodes As TreeNodeCollection) As TreeNode
        For Each n As TreeNode In nodes
            If n.Text = value Then
                Return n
            Else
                'Recursively call the Function
                Dim nodeToFind As TreeNode = FindNodeByValue(value, n.Nodes)
                If nodeToFind IsNot Nothing Then
                    Return nodeToFind
                End If
            End If
        Next

        Return Nothing
    End Function
    Private Sub Event_Update_List(sender As Object, e As EventArgs) Handles Timer_Update_List.Tick
        Dim SelectedNode As TreeNode = TView_UPSVar.SelectedNode
        If SelectedNode IsNot Nothing Then
            If SelectedNode.Parent IsNot Nothing Then
                If SelectedNode.Parent.Text <> UPS_Name And SelectedNode.Nodes.Count = 0 Then
                    Dim VarName = Replace(TView_UPSVar.SelectedNode.FullPath, UPS_Name & ".", "")
                    LogFile.LogTracing("Update {VarName}", LogLvl.LOG_DEBUG, Me)
                    Lbl_V_Value.Text = WinNUT.UPS_Device.GetUPSVar(VarName)
                End If
            End If
        End If
    End Sub

    Private Sub Btn_Close_Click(sender As Object, e As EventArgs) Handles Btn_Close.Click
        LogFile.LogTracing("Close List Var Gui", LogLvl.LOG_DEBUG, Me)
        Close()
    End Sub

    Private Sub Btn_Reload_Click(sender As Object, e As EventArgs) Handles Btn_Reload.Click
        LogFile.LogTracing("Reload Treeview from Button", LogLvl.LOG_DEBUG, Me)
        Lbl_N_Value.Text = ""
        Lbl_V_Value.Text = ""
        Lbl_D_Value.Text = ""
        TView_UPSVar.Nodes.Clear()
        PopulateTreeView()
    End Sub

    Private Sub TView_UPSVar_NodeChanged(sender As Object, e As TreeViewEventArgs) Handles TView_UPSVar.AfterSelect
        Dim index As Integer = 0
        Dim UPSName = My.Settings.NUT_UPSName
        Dim SelectedChild = Replace(e.Node.FullPath, UPSName & ".", "", 1, 1)
        Dim FindChild As Predicate(Of UPS_List_Datas) = Function(ByVal x As UPS_List_Datas)
                                                            If x.VarKey = SelectedChild Then
                                                                Return True
                                                            Else
                                                                index += 1
                                                                Return False
                                                            End If
                                                        End Function
        If Not SelectedChild = UPSName And List_Var_Datas.FindIndex(FindChild) <> -1 Then
            LogFile.LogTracing("Select {List_Var_Datas.Item(index).VarKey} Node", LogLvl.LOG_DEBUG, Me)
            Lbl_N_Value.Text = List_Var_Datas.Item(index).VarKey
            Lbl_V_Value.Text = List_Var_Datas.Item(index).VarValue
            Lbl_D_Value.Text = List_Var_Datas.Item(index).VarDesc
        Else
            Lbl_N_Value.Text = ""
            Lbl_V_Value.Text = ""
            Lbl_D_Value.Text = ""
        End If
    End Sub

    Private Function SerializeUPSData() As String
        LogFile.LogTracing("Serializing UPS data to String.", LogLvl.LOG_DEBUG, Me)
        Dim sb As New Text.StringBuilder()

        With WinNUT.UPS_Device.UPS_Datas
            sb.AppendLine(UPSDevice.Name & " (" & .Mfr & "/" & .Model & "/" & .Firmware & ")")
        End With

        For Each LDatas In List_Var_Datas
            sb.AppendLine(LDatas.VarKey & " (" & LDatas.VarDesc & ") : " & LDatas.VarValue)
        Next

        LogFile.LogTracing("Successfully built serialized string, length: " & sb.Length, LogLvl.LOG_DEBUG, Me)
        Return sb.ToString()
    End Function

    Private Sub Btn_Clip_Click(sender As Object, e As EventArgs) Handles Btn_Clip.Click
        LogFile.LogTracing("Copy TreeView To Clipboard", LogLvl.LOG_DEBUG, Me)

        Try
            Clipboard.SetText(SerializeUPSData)
            LogFile.LogTracing("Successfully copied UPS information to the Clipboard.", LogLvl.LOG_NOTICE, Me,
                                My.Resources.List_Var_Gui__SetCpbTextSuccess)

        Catch ex As Exception
            Dim frmtdError = String.Format(My.Resources.List_Var_Gui__SetCpbTextError_Text, ex.Message)
            LogFile.LogTracing("Exception encountered while attempting to set Clipboard text.", LogLvl.LOG_ERROR, Me,
                            frmtdError)
            LogFile.LogException(ex, Me)
            MessageBox.Show(frmtdError, My.Resources.List_Var_Gui__SetCpbTextError_Caption,
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Btn_Save_Click(sender As Object, e As EventArgs) Handles Btn_Save.Click
        LogFile.LogTracing("Export TreeView To File", LogLvl.LOG_DEBUG, Me)

        Dim sfd As New SaveFileDialog With {
            .Filter = "Text files|*.txt|All files|*.*",
            .InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            .Title = My.Resources.List_Var_Gui__SaveFile_Caption
        }
        Dim dialogRes = sfd.ShowDialog()

        If dialogRes = DialogResult.OK AndAlso sfd.FileName <> String.Empty Then
            LogFile.LogTracing("User completed SaveFileDialog, path: " & sfd.FileName, LogLvl.LOG_NOTICE, Me)
            Try
                Using sw As New IO.StreamWriter(sfd.OpenFile())
                    sw.Write(SerializeUPSData)
                    sw.Close()
                    LogFile.LogTracing("File saved successfully.", LogLvl.LOG_NOTICE, Me,
                                        String.Format(My.Resources.List_Var_Gui__SaveFileSuccess, sfd.FileName))
                End Using

            Catch ex As Exception
                Dim frmtdError = String.Format(My.Resources.List_Var_Gui__SaveFileError_Text, ex.Message)
                LogFile.LogTracing("Exception encountered while saving UPS data to a file.", LogLvl.LOG_ERROR, Me,
                                    frmtdError)
                LogFile.LogException(ex, Me)
                MessageBox.Show(frmtdError, My.Resources.List_Var_Gui__SaveFile_Caption,
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Else
            LogFile.LogTracing("SaveFileDialog was not accepted.", LogLvl.LOG_NOTICE, Me)
        End If
    End Sub

    Function GetChildren(parentNode As TreeNode) As List(Of String)
        Dim nodes As List(Of String) = New List(Of String)
        GetAllChildren(parentNode, nodes)
        Return nodes
    End Function

    Sub GetAllChildren(parentNode As TreeNode, nodes As List(Of String))
        For Each childNode As TreeNode In parentNode.Nodes
            nodes.Add(childNode.Text)
            GetAllChildren(childNode, nodes)
        Next
    End Sub
End Class
