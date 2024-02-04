' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

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
    Private Sub Btn_Clear_Click(sender As Object, e As EventArgs) Handles Btn_Clear.Click
        TView_UPSVar.CollapseAll()
        Lbl_N_Value.Text = ""
        Lbl_V_Value.Text = ""
        Lbl_D_Value.Text = ""
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

    Private Sub Btn_Clip_Click(sender As Object, e As EventArgs) Handles Btn_Clip.Click
        LogFile.LogTracing("Export TreeView To Clipboard", LogLvl.LOG_DEBUG, Me)
        Dim ToClipBoard As String = Nothing
        With WinNUT.UPS_Device.UPS_Datas
            ToClipBoard = My.Settings.NUT_UPSName & " (" & .Mfr & "/" & .Model & "/" & .Firmware & ")" & vbNewLine
        End With
        For Each LDatas In List_Var_Datas
            ToClipBoard &= LDatas.VarKey & " (" & LDatas.VarDesc & ") : " & LDatas.VarValue & vbNewLine
        Next
        My.Computer.Clipboard.SetText(ToClipboard)
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
