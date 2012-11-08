<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FieldEditor.aspx.cs" Inherits="Sitecore.Shell.Applications.ContentManager.FieldEditorPage" %>
<%@ Import Namespace="Sitecore.Globalization"%>
<%@ Register TagPrefix="sc" Namespace="Sitecore.Web.UI.HtmlControls" Assembly="Sitecore.Kernel" %>
<%@ Register TagPrefix="rad" Namespace="Telerik.Web.UI" %>
<asp:PlaceHolder id="DocumentType" runat="server" />

<html>
<head runat="server">
  <asp:placeholder id="BrowserTitle" runat="server" />
  <sc:Stylesheet runat="server" Src="Content Manager.css" DeviceDependant="true"/>
  <asp:placeholder id="Stylesheets" runat="server" />

	<sc:Stylesheet ID="Stylesheet1" runat="server" Src="/sitecore modules/shell/field suite/styles/fieldsuite.fields.css"/>
	<sc:Stylesheet ID="Stylesheet2" runat="server" Src="/sitecore modules/shell/field suite/styles/fieldsuite.fields.images.css"/>
	<sc:Stylesheet ID="Stylesheet3" runat="server" Src="/sitecore modules/shell/field suite/styles/fieldsuite.fields.droplink.css"/>
	<sc:Stylesheet ID="Stylesheet4" runat="server" Src="/sitecore modules/shell/field suite/styles/fieldsuite.fields.treelist.css"/>

	<script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/SitecoreObjects.js"></script>
	<script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/SitecoreKeyboard.js"></script>
	<script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/SitecoreVSplitter.js"></script>
	<script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/SitecoreWindow.js"></script>

	<script type="text/JavaScript" language="javascript" src="/sitecore/shell/Applications/Content Manager/Content Editor.js"></script>
	<script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/TreeviewEx/TreeviewEx.js"></script>  
	<script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/SitecoreModifiedHandling.js"></script>

	<script type="text/JavaScript" language="javascript" src="/sitecore modules/shell/field suite/scripts/fieldsuite.fields.js"></script>
	<script type="text/JavaScript" language="javascript" src="/sitecore modules/shell/field suite/scripts/fieldsuite.html.js"></script>
	<script type="text/JavaScript" language="javascript" src="/sitecore modules/shell/field suite/scripts/fieldsuite.fields.images.js"></script>
	<script type="text/JavaScript" language="javascript" src="/sitecore modules/shell/field suite/scripts/fieldsuite.fields.droplink.js"></script>
	<script type="text/JavaScript" language="javascript" src="/sitecore modules/shell/field suite/scripts/fieldsuite.fields.droptree.js"></script>
	<script type="text/JavaScript" language="javascript" src="/sitecore modules/shell/field suite/scripts/fieldsuite.fields.treelist.js"></script>
  
  <script type="text/javascript">
    function OnResize() {
      var header = $("HeaderRow");
      var footer = $("FooterRow");

      var editor = $("EditorPanel");
      
      var height = window.innerHeight - header.getHeight() - footer.getHeight() + 'px';

      editor.setStyle({ height: height });
    }

    if (Prototype.Browser.Gecko) {
      Event.observe(window, "load", OnResize);
      Event.observe(window, "resize", OnResize);
    }
  </script>
  
  <style type="text/css">
    #Editors, #MainPanel {
        background: #f0f1f2 !important;  
    }
    
    #EditorPanel, .scEditorPanelCell {
  /*
      background-color: #f0f1f2;
      border: none;
  */
      padding-bottom: 1px;
    }
    
    .scEditorPanelCell {
      padding-bottom: 1px;
    }
    
    .ie #ValidatorPanel {
      margin-top: -1px;
    }
    
    .scEditorSections {
      /* border-bottom: none; */
      margin-right: -1px;
      background: blue;
    }
    
    .ff .scEditorSections {
      margin-top: -2px;
      margin-right: 1px;
    }
    
    #HeaderRow {
      display: none;
    }
    
    #FooterRow {
    }
    
    #FooterRow td {
      border-top: solid 1px #DBDBDB;
    }
    
    #FooterRow input {
      margin-right: 4px;
    }
    
    .scEditorSectionPanelCell {
      padding-left: 8px;
    }
    
    .scEditorSectionCaptionExpanded {
  padding: 1px 2px 1px 2px;
    }
    
    .scButton {
      font:8pt tahoma;
    }

    #WarningRow td
    {
       background: #ffffe4;      
       padding: 2px;
       font-weight: bolder;
    }
  
  </style>
</head>
<body runat="server" id="Body" 
  onmousedown="javascript:scWin.mouseDown(this, event)"
  onmousemove="javascript:scWin.mouseMove(this, event)"
  onmouseup="javascript:scWin.mouseUp(this, event)" style="background-color: #e9e9e9">
  <form id="ContentEditorForm" runat="server"><sc:CodeBeside runat="server" Type="Sitecore.Shell.Applications.ContentManager.FieldEditorForm,Sitecore.Client"/>
  <asp:PlaceHolder id="scLanguage" runat="server" />

  <input type="hidden" id="scEditorTabs" name="scEditorTabs" />
  <input type="hidden" id="scActiveEditorTab" name="scActiveEditorTab" />
  <input type="hidden" id="scPostAction" name="scPostAction" />
  <input type="hidden" id="scShowEditor" name="scShowEditor" />
  <input type="hidden" id="scSections" name="scSections" />
  <div id="outline" class="scOutline" style="display:none"></div>
  <span id="scPostActionText" style="display:none"><sc:Literal ID="Literal1" Text="The main window could not be updated due to the current browser security settings. You must click the Refresh button yourself to view the changes." runat="server" /></span>
  <iframe id="feRTEContainer" src="about:blank" style="position: absolute; width: 100%; height: 100%; top: 0px; left: 0px; right: 0px; bottom: 0px; z-index: 999;border:none; display:none" frameborder="0" allowtransparency="allowtransparency"></iframe>                                                                                                                                        
  <table height="100%" class="scPanel" cellpadding="0" cellspacing="0" border="0" onactivate="javascript:scWin.activate(this, event)">
    <tr id="HeaderRow">
      <td>
        <table cellpadding=0 cellspacing=0 style="background: white">
          <tr>
            <td><sc:ThemedImage Margin="4px 8px 4px 8px" ID="DialogIcon" Src="people/32x32/cubes_blue.png" runat="server" Height="32" Width="32" /></td>
            
            <td valign="top" width="100%">
              <div style="padding: 2px 16px 0px 0px;">
                <div style="padding: 0px 0px 4px 0px; font: bold 9pt tahoma; color: black"><sc:Literal Text="Field Editor" ID="DialogTitle" runat="server" /></div>
                <div style="color: #333333"><sc:Literal ID="DialogText" Text="Edit the fields" runat="server" /></div>
              </div>
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr runat="server" visible="false" id="WarningRow">
      <td>
        <sc:ThemedImage runat="server" Height="16" Width="16" style="vertical-align:middle; margin-right: 4px" Src="Applications/16x16/warning.png" /><asp:Literal runat="server" ID="warningText"></asp:Literal>
      </td>
    </tr>
    <tr height="100%">
      <td height="100%" id="MainPanel" class="scDockMain" onclick="javascript:scContent.onEditorClick(this, event)" valign="top">
        <table class="scPanel" cellpadding="0" cellspacing="0" border="0">
          <tr>
            <td id="MainContent" valign="top">
              <sc:Border ID="ContentEditor" runat="server" Class="scEditor" style="margin-top: -1px"/>
            </td>
         </tr>
        </table>
      </td>
    </tr>
    
    <tr id="FooterRow">
      <td align="right" style="padding: 4px 4px 4px 0">
        <asp:Literal runat="server" ID="Buttons" />
      </td>
    </tr>
  </table>
  <sc:KeyMap runat="server" />
  </form>
</body>
</html>
