﻿#pragma checksum "..\..\BaseConverterControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "4864E83CB9299552BB2115837731E4CF"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Menees.VsTools {
    
    
    internal partial class BaseConverterControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 39 "..\..\BaseConverterControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox byteOrder;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\BaseConverterControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox numberType;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\BaseConverterControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox hexEdit;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\BaseConverterControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox decimalEdit;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\BaseConverterControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox binaryEdit;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Menees.VsTools;component/baseconvertercontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\BaseConverterControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 9 "..\..\BaseConverterControl.xaml"
            ((Menees.VsTools.BaseConverterControl)(target)).Loaded += new System.Windows.RoutedEventHandler(this.BaseConverterControl_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.byteOrder = ((System.Windows.Controls.ComboBox)(target));
            
            #line 39 "..\..\BaseConverterControl.xaml"
            this.byteOrder.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ByteOrder_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.numberType = ((System.Windows.Controls.ComboBox)(target));
            
            #line 46 "..\..\BaseConverterControl.xaml"
            this.numberType.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DataType_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.hexEdit = ((System.Windows.Controls.TextBox)(target));
            
            #line 61 "..\..\BaseConverterControl.xaml"
            this.hexEdit.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.TextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.decimalEdit = ((System.Windows.Controls.TextBox)(target));
            
            #line 63 "..\..\BaseConverterControl.xaml"
            this.decimalEdit.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.TextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.binaryEdit = ((System.Windows.Controls.TextBox)(target));
            
            #line 65 "..\..\BaseConverterControl.xaml"
            this.binaryEdit.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.TextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

