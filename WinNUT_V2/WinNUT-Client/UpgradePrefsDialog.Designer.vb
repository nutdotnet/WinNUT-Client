' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Namespace Forms

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
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UpgradePrefsDialog))
            Me.PictureBox1 = New System.Windows.Forms.PictureBox()
            Me.IntroMessage = New System.Windows.Forms.Label()
            Me.OK_Button = New System.Windows.Forms.Button()
            Me.UpgradePrefsDialogModelBindingSource = New System.Windows.Forms.BindingSource(Me.components)
            Me.Cancel_Button = New System.Windows.Forms.Button()
            Me.ImportSettingsCheckBox = New System.Windows.Forms.CheckBox()
            Me.DeleteSettingsCheckBox = New System.Windows.Forms.CheckBox()
            Me.PrevSettngsGroupBox = New System.Windows.Forms.GroupBox()
            Me.BackupSettingsCheckbox = New System.Windows.Forms.CheckBox()
            Me.buttonPanel = New System.Windows.Forms.Panel()
            Me.UpgradeProgressBar = New System.Windows.Forms.ProgressBar()
            Me.TopContentPanel = New System.Windows.Forms.Panel()
            Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
            CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.UpgradePrefsDialogModelBindingSource, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.PrevSettngsGroupBox.SuspendLayout()
            Me.buttonPanel.SuspendLayout()
            Me.TopContentPanel.SuspendLayout()
            Me.SuspendLayout()
            '
            'PictureBox1
            '
            Me.PictureBox1.Image = Global.WinNUT_Client.My.Resources.Resources.XP_Information
            resources.ApplyResources(Me.PictureBox1, "PictureBox1")
            Me.PictureBox1.Name = "PictureBox1"
            Me.PictureBox1.TabStop = False
            '
            'IntroMessage
            '
            resources.ApplyResources(Me.IntroMessage, "IntroMessage")
            Me.IntroMessage.Name = "IntroMessage"
            '
            'OK_Button
            '
            resources.ApplyResources(Me.OK_Button, "OK_Button")
            Me.OK_Button.DataBindings.Add(New System.Windows.Forms.Binding("Enabled", Me.UpgradePrefsDialogModelBindingSource, "OKButtonEnabled", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.OK_Button.Name = "OK_Button"
            '
            'UpgradePrefsDialogModelBindingSource
            '
            Me.UpgradePrefsDialogModelBindingSource.DataSource = GetType(WinNUT_Client.Models.UpgradePrefsDialogModel)
            '
            'Cancel_Button
            '
            resources.ApplyResources(Me.Cancel_Button, "Cancel_Button")
            Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.Cancel_Button.Name = "Cancel_Button"
            '
            'ImportSettingsCheckBox
            '
            resources.ApplyResources(Me.ImportSettingsCheckBox, "ImportSettingsCheckBox")
            Me.ImportSettingsCheckBox.Checked = True
            Me.ImportSettingsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked
            Me.ImportSettingsCheckBox.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.UpgradePrefsDialogModelBindingSource, "ImportPreviousSettigns", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.ImportSettingsCheckBox.Name = "ImportSettingsCheckBox"
            Me.ToolTip1.SetToolTip(Me.ImportSettingsCheckBox, resources.GetString("ImportSettingsCheckBox.ToolTip"))
            Me.ImportSettingsCheckBox.UseVisualStyleBackColor = True
            '
            'DeleteSettingsCheckBox
            '
            resources.ApplyResources(Me.DeleteSettingsCheckBox, "DeleteSettingsCheckBox")
            Me.DeleteSettingsCheckBox.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.UpgradePrefsDialogModelBindingSource, "DeletePreviousSettings", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.DeleteSettingsCheckBox.Name = "DeleteSettingsCheckBox"
            Me.ToolTip1.SetToolTip(Me.DeleteSettingsCheckBox, resources.GetString("DeleteSettingsCheckBox.ToolTip"))
            Me.DeleteSettingsCheckBox.UseVisualStyleBackColor = True
            '
            'PrevSettngsGroupBox
            '
            resources.ApplyResources(Me.PrevSettngsGroupBox, "PrevSettngsGroupBox")
            Me.PrevSettngsGroupBox.Controls.Add(Me.ImportSettingsCheckBox)
            Me.PrevSettngsGroupBox.Controls.Add(Me.BackupSettingsCheckbox)
            Me.PrevSettngsGroupBox.Controls.Add(Me.DeleteSettingsCheckBox)
            Me.PrevSettngsGroupBox.Name = "PrevSettngsGroupBox"
            Me.PrevSettngsGroupBox.TabStop = False
            '
            'BackupSettingsCheckbox
            '
            resources.ApplyResources(Me.BackupSettingsCheckbox, "BackupSettingsCheckbox")
            Me.BackupSettingsCheckbox.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Me.UpgradePrefsDialogModelBindingSource, "BackupPreviousSettings", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.BackupSettingsCheckbox.Name = "BackupSettingsCheckbox"
            Me.ToolTip1.SetToolTip(Me.BackupSettingsCheckbox, resources.GetString("BackupSettingsCheckbox.ToolTip"))
            Me.BackupSettingsCheckbox.UseVisualStyleBackColor = True
            '
            'buttonPanel
            '
            Me.buttonPanel.Controls.Add(Me.Cancel_Button)
            Me.buttonPanel.Controls.Add(Me.OK_Button)
            resources.ApplyResources(Me.buttonPanel, "buttonPanel")
            Me.buttonPanel.Name = "buttonPanel"
            '
            'UpgradeProgressBar
            '
            resources.ApplyResources(Me.UpgradeProgressBar, "UpgradeProgressBar")
            Me.UpgradeProgressBar.DataBindings.Add(New System.Windows.Forms.Binding("Value", Me.UpgradePrefsDialogModelBindingSource, "ProgressPercent", True))
            Me.UpgradeProgressBar.Name = "UpgradeProgressBar"
            '
            'TopContentPanel
            '
            Me.TopContentPanel.Controls.Add(Me.PictureBox1)
            Me.TopContentPanel.Controls.Add(Me.IntroMessage)
            resources.ApplyResources(Me.TopContentPanel, "TopContentPanel")
            Me.TopContentPanel.Name = "TopContentPanel"
            '
            'UpgradePrefsDialog
            '
            Me.AcceptButton = Me.OK_Button
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.CancelButton = Me.Cancel_Button
            Me.ControlBox = False
            Me.Controls.Add(Me.TopContentPanel)
            Me.Controls.Add(Me.PrevSettngsGroupBox)
            Me.Controls.Add(Me.buttonPanel)
            Me.Controls.Add(Me.UpgradeProgressBar)
            Me.DataBindings.Add(New System.Windows.Forms.Binding("Enabled", Me.UpgradePrefsDialogModelBindingSource, "FormEnabled", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.DataBindings.Add(New System.Windows.Forms.Binding("Icon", Me.UpgradePrefsDialogModelBindingSource, "Icon", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "UpgradePrefsDialog"
            Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
            CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.UpgradePrefsDialogModelBindingSource, System.ComponentModel.ISupportInitialize).EndInit()
            Me.PrevSettngsGroupBox.ResumeLayout(False)
            Me.PrevSettngsGroupBox.PerformLayout()
            Me.buttonPanel.ResumeLayout(False)
            Me.TopContentPanel.ResumeLayout(False)
            Me.TopContentPanel.PerformLayout()
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents OK_Button As System.Windows.Forms.Button
        Friend WithEvents Cancel_Button As System.Windows.Forms.Button
        Friend WithEvents PictureBox1 As PictureBox
        Friend WithEvents IntroMessage As Label
        Friend WithEvents ImportSettingsCheckBox As CheckBox
        Friend WithEvents DeleteSettingsCheckBox As CheckBox
        Friend WithEvents PrevSettngsGroupBox As GroupBox
        Friend WithEvents buttonPanel As Panel
        Private WithEvents UpgradeProgressBar As ProgressBar
        Friend WithEvents TopContentPanel As Panel
        Friend WithEvents ToolTip1 As ToolTip
        Friend WithEvents UpgradePrefsDialogModelBindingSource As BindingSource
        Friend WithEvents BackupSettingsCheckbox As CheckBox
    End Class

End Namespace
