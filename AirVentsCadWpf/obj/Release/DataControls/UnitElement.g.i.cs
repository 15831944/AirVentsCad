﻿#pragma checksum "..\..\..\DataControls\UnitElement.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "D13378E07A9419E1C1CFBA988F79EE5E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
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


namespace AirVentsCadWpf.DataControls {
    
    
    /// <summary>
    /// UnitElement
    /// </summary>
    public partial class UnitElement : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 22 "..\..\..\DataControls\UnitElement.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border Unit;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\DataControls\UnitElement.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid GridR;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\DataControls\UnitElement.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid GridL;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\DataControls\UnitElement.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid GridForm;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\DataControls\UnitElement.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label WidthUnitS;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\..\DataControls\UnitElement.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label WidthUnit;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\DataControls\UnitElement.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border BorderR;
        
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
            System.Uri resourceLocater = new System.Uri("/AirVentsCad;component/datacontrols/unitelement.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\DataControls\UnitElement.xaml"
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
            
            #line 6 "..\..\..\DataControls\UnitElement.xaml"
            ((AirVentsCadWpf.DataControls.UnitElement)(target)).SizeChanged += new System.Windows.SizeChangedEventHandler(this.UserControl_SizeChanged_1);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Unit = ((System.Windows.Controls.Border)(target));
            
            #line 22 "..\..\..\DataControls\UnitElement.xaml"
            this.Unit.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Border_MouseDown_3);
            
            #line default
            #line hidden
            
            #line 22 "..\..\..\DataControls\UnitElement.xaml"
            this.Unit.MouseEnter += new System.Windows.Input.MouseEventHandler(this.Unit_MouseEnter);
            
            #line default
            #line hidden
            
            #line 22 "..\..\..\DataControls\UnitElement.xaml"
            this.Unit.MouseLeave += new System.Windows.Input.MouseEventHandler(this.Unit_MouseLeave);
            
            #line default
            #line hidden
            
            #line 22 "..\..\..\DataControls\UnitElement.xaml"
            this.Unit.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Unit_MouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 22 "..\..\..\DataControls\UnitElement.xaml"
            this.Unit.SizeChanged += new System.Windows.SizeChangedEventHandler(this.Unit_SizeChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 25 "..\..\..\DataControls\UnitElement.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.GridR = ((System.Windows.Controls.Grid)(target));
            return;
            case 5:
            this.GridL = ((System.Windows.Controls.Grid)(target));
            return;
            case 6:
            this.GridForm = ((System.Windows.Controls.Grid)(target));
            return;
            case 7:
            this.WidthUnitS = ((System.Windows.Controls.Label)(target));
            return;
            case 8:
            this.WidthUnit = ((System.Windows.Controls.Label)(target));
            return;
            case 9:
            this.BorderR = ((System.Windows.Controls.Border)(target));
            
            #line 41 "..\..\..\DataControls\UnitElement.xaml"
            this.BorderR.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Border_MouseDown_1);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 42 "..\..\..\DataControls\UnitElement.xaml"
            ((System.Windows.Controls.Border)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Border_MouseDown_2);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

