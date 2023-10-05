' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UpgradePrefsDialog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UpgradePrefsDialog))
        Me.primaryActions = New System.Windows.Forms.TableLayoutPanel()
        Me.OK_Button = New System.Windows.Forms.Button()
        Me.Cancel_Button = New System.Windows.Forms.Button()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TopContentTlp = New System.Windows.Forms.TableLayoutPanel()
        Me.ButtonOpenRegedit = New System.Windows.Forms.Button()
        Me.ImportSettingsCheckBox = New System.Windows.Forms.CheckBox()
        Me.DeleteSettingsCheckBox = New System.Windows.Forms.CheckBox()
        Me.PrevSettngsGroupBox = New System.Windows.Forms.GroupBox()
        Me.mainFlowLayoutPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.buttonPanel = New System.Windows.Forms.Panel()
        Me.primaryActions.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TopContentTlp.SuspendLayout()
        Me.PrevSettngsGroupBox.SuspendLayout()
        Me.mainFlowLayoutPanel.SuspendLayout()
        Me.buttonPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'primaryActions
        '
        Me.primaryActions.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.primaryActions.ColumnCount = 2
        Me.primaryActions.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.primaryActions.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.primaryActions.Controls.Add(Me.OK_Button, 0, 0)
        Me.primaryActions.Controls.Add(Me.Cancel_Button, 1, 0)
        Me.primaryActions.Location = New System.Drawing.Point(285, 0)
        Me.primaryActions.Margin = New System.Windows.Forms.Padding(0)
        Me.primaryActions.Name = "primaryActions"
        Me.primaryActions.RowCount = 1
        Me.primaryActions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.primaryActions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.primaryActions.Size = New System.Drawing.Size(146, 29)
        Me.primaryActions.TabIndex = 0
        '
        'OK_Button
        '
        Me.OK_Button.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.OK_Button.Location = New System.Drawing.Point(3, 3)
        Me.OK_Button.Name = "OK_Button"
        Me.OK_Button.Size = New System.Drawing.Size(67, 23)
        Me.OK_Button.TabIndex = 0
        Me.OK_Button.Text = "OK"
        '
        'Cancel_Button
        '
        Me.Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel_Button.Location = New System.Drawing.Point(76, 3)
        Me.Cancel_Button.Name = "Cancel_Button"
        Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
        Me.Cancel_Button.TabIndex = 1
        Me.Cancel_Button.Text = "Cancel"
        '
        'PictureBox1
        '
        Me.PictureBox1.Image = Global.WinNUT_Client.My.Resources.Resources.XP_Information
        Me.PictureBox1.Location = New System.Drawing.Point(6, 6)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(47, 48)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.PictureBox1.TabIndex = 1
        Me.PictureBox1.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(59, 3)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(372, 108)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = resources.GetString("Label1.Text")
        '
        'TopContentTlp
        '
        Me.TopContentTlp.ColumnCount = 2
        Me.TopContentTlp.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 53.0!))
        Me.TopContentTlp.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TopContentTlp.Controls.Add(Me.PictureBox1, 0, 0)
        Me.TopContentTlp.Controls.Add(Me.Label1, 1, 0)
        Me.TopContentTlp.Location = New System.Drawing.Point(0, 0)
        Me.TopContentTlp.Margin = New System.Windows.Forms.Padding(0, 0, 0, 3)
        Me.TopContentTlp.Name = "TopContentTlp"
        Me.TopContentTlp.Padding = New System.Windows.Forms.Padding(3)
        Me.TopContentTlp.RowCount = 1
        Me.TopContentTlp.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TopContentTlp.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.TopContentTlp.Size = New System.Drawing.Size(437, 114)
        Me.TopContentTlp.TabIndex = 3
        '
        'ButtonOpenRegedit
        '
        Me.ButtonOpenRegedit.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ButtonOpenRegedit.Location = New System.Drawing.Point(3, 3)
        Me.ButtonOpenRegedit.Name = "ButtonOpenRegedit"
        Me.ButtonOpenRegedit.Size = New System.Drawing.Size(84, 23)
        Me.ButtonOpenRegedit.TabIndex = 4
        Me.ButtonOpenRegedit.Text = "Open RegEdit"
        Me.ButtonOpenRegedit.UseVisualStyleBackColor = True
        '
        'ImportSettingsCheckBox
        '
        Me.ImportSettingsCheckBox.AutoSize = True
        Me.ImportSettingsCheckBox.Location = New System.Drawing.Point(6, 19)
        Me.ImportSettingsCheckBox.Name = "ImportSettingsCheckBox"
        Me.ImportSettingsCheckBox.Size = New System.Drawing.Size(55, 17)
        Me.ImportSettingsCheckBox.TabIndex = 5
        Me.ImportSettingsCheckBox.Text = "Import"
        Me.ImportSettingsCheckBox.UseVisualStyleBackColor = True
        '
        'DeleteSettingsCheckBox
        '
        Me.DeleteSettingsCheckBox.AutoSize = True
        Me.DeleteSettingsCheckBox.Location = New System.Drawing.Point(6, 42)
        Me.DeleteSettingsCheckBox.Name = "DeleteSettingsCheckBox"
        Me.DeleteSettingsCheckBox.Size = New System.Drawing.Size(57, 17)
        Me.DeleteSettingsCheckBox.TabIndex = 6
        Me.DeleteSettingsCheckBox.Text = "Delete"
        Me.DeleteSettingsCheckBox.UseVisualStyleBackColor = True
        '
        'PrevSettngsGroupBox
        '
        Me.PrevSettngsGroupBox.Controls.Add(Me.DeleteSettingsCheckBox)
        Me.PrevSettngsGroupBox.Controls.Add(Me.ImportSettingsCheckBox)
        Me.PrevSettngsGroupBox.Dock = System.Windows.Forms.DockStyle.Top
        Me.PrevSettngsGroupBox.Location = New System.Drawing.Point(3, 126)
        Me.PrevSettngsGroupBox.Margin = New System.Windows.Forms.Padding(3, 9, 3, 3)
        Me.PrevSettngsGroupBox.Name = "PrevSettngsGroupBox"
        Me.PrevSettngsGroupBox.Size = New System.Drawing.Size(431, 79)
        Me.PrevSettngsGroupBox.TabIndex = 7
        Me.PrevSettngsGroupBox.TabStop = False
        Me.PrevSettngsGroupBox.Text = "Previous Settings"
        '
        'mainFlowLayoutPanel
        '
        Me.mainFlowLayoutPanel.Controls.Add(Me.TopContentTlp)
        Me.mainFlowLayoutPanel.Controls.Add(Me.PrevSettngsGroupBox)
        Me.mainFlowLayoutPanel.Controls.Add(Me.buttonPanel)
        Me.mainFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.mainFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.mainFlowLayoutPanel.Location = New System.Drawing.Point(0, 0)
        Me.mainFlowLayoutPanel.Name = "mainFlowLayoutPanel"
        Me.mainFlowLayoutPanel.Size = New System.Drawing.Size(437, 243)
        Me.mainFlowLayoutPanel.TabIndex = 7
        Me.mainFlowLayoutPanel.WrapContents = False
        '
        'buttonPanel
        '
        Me.buttonPanel.Controls.Add(Me.ButtonOpenRegedit)
        Me.buttonPanel.Controls.Add(Me.primaryActions)
        Me.buttonPanel.Dock = System.Windows.Forms.DockStyle.Top
        Me.buttonPanel.Location = New System.Drawing.Point(3, 211)
        Me.buttonPanel.Name = "buttonPanel"
        Me.buttonPanel.Size = New System.Drawing.Size(431, 32)
        Me.buttonPanel.TabIndex = 7
        '
        'UpgradePrefsDialog
        '
        Me.AcceptButton = Me.OK_Button
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Cancel_Button
        Me.ClientSize = New System.Drawing.Size(437, 243)
        Me.ControlBox = False
        Me.Controls.Add(Me.mainFlowLayoutPanel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "UpgradePrefsDialog"
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Migrate to New Settings Format"
        Me.primaryActions.ResumeLayout(False)
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TopContentTlp.ResumeLayout(False)
        Me.TopContentTlp.PerformLayout()
        Me.PrevSettngsGroupBox.ResumeLayout(False)
        Me.PrevSettngsGroupBox.PerformLayout()
        Me.mainFlowLayoutPanel.ResumeLayout(False)
        Me.buttonPanel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents primaryActions As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents TopContentTlp As TableLayoutPanel
    Friend WithEvents ButtonOpenRegedit As Button
    Friend WithEvents ImportSettingsCheckBox As CheckBox
    Friend WithEvents DeleteSettingsCheckBox As CheckBox
    Friend WithEvents PrevSettngsGroupBox As GroupBox
    Friend WithEvents mainFlowLayoutPanel As FlowLayoutPanel
    Friend WithEvents buttonPanel As Panel
End Class
