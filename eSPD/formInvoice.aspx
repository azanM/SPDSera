<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPage.Master" CodeBehind="formInvoice.aspx.cs" Inherits="eSPD.formInvoice" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .hideIt {
            display: none;
        }

        .fixit {
            width: 100%;
        }

        .fright {
            text-align: right;
        }

        .GridPager a {
            display: block;
            height: 15px;
            width: 15px;
            color: #fff;
            font-weight: bold;
            border: 1px solid #e3e3e3;
            text-align: center;
            text-decoration: none;
        }

        .GridPager span {
            display: block;
            height: 15px;
            width: 15px;
            color: #000;
            font-weight: bold;
            border: 1px solid #e3e3e3;
            text-align: center;
            background-color: #E3EAEB;
            text-decoration: none;
        }

        h3 {
            border-bottom: 1px solid #e3e3e3;
            padding-bottom: 5px;
        }

            h3 span {
                float: right;
            }

        .labelMessage {
            padding: 1em;
            background-color: #dcddde;
            border: 1px solid #ddd;
            display: block;
            font-weight: 300;
        }

        .timepicker {
            border-style:solid;
            border-left:unset;
            border-color:gray;
        
        }
        .datepicker {
            border-style:solid;
            
            border-color:gray;
        
        }

        .text {
            border-style:solid;
            
            border-color:gray;
        }

        .loading {
            background-color: white;
            bottom: 0;
            color: black;
            display: none;
            font-size: 4em;
            left: 0;
            opacity: 0.5;
            -ms-filter: "progid:DXImageTransform.Microsoft.Alpha(Opacity=50)";
            /* IE 5-7 */
            filter: alpha(opacity=50);
            /* Netscape */
            -moz-opacity: 0.5;
            /* Safari 1.x */
            -khtml-opacity: 0.5;
            padding: 6em;
            position: fixed;
            right: 0;
            top: 0;
            z-index: 10000;
            float: right;
            text-align: right;
        }
    </style>
    <script type="text/javascript">
        function openDetail(url) {
            window.open(url, "", "width=800, height=600, scrollbars=yes, resizable=yes");
        }

        function Hidepopup() {
            $find("popupMessage").Hide();
            return false;
        }

        function ShowLoading() {
            $('.loading').fadeIn();
        }

        function CloseLoading() {
            $('.loading').fadeOut();
        }

        function errorMessage(message)
        {
            alert(message);
        }
        /*
        $(function () {
            $(".calenderJQ").datepicker();
        }); */
        
        //$(function () {
        //    jQuery('.datetimepicker').datetimepicker({
        //        step: 15,
        //        dateFormat: 'YYYY/mm/dd H:mm ',
        //    });
        //});

        $(function () {
            jQuery('.datepicker').datetimepicker({
                timepicker: false,
                format: 'Y/m/d',
               
            });
        });

        $(function () {
            jQuery('.timepicker').datetimepicker({
                datepicker: false,
                format: 'H:i',
                step: 15
               
            });
        });

    </script>
    <link rel="stylesheet"  type="text/css" href="Script/jquery.datetimepicker.css" />
    <script type="text/javascript" src="Script/jquery.js"></script>
    <script type="text/javascript" src="Script/jquery.datetimepicker.full.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="loading">Please Wait</div>
    <br />
    <p style="font-size:large;font-weight:600">Invoice SPD</p>
    Nomor SPD : &nbsp&nbsp&nbsp&nbsp
    <asp:TextBox ID="txtNomorSPDSearch" runat="server" AutoPostBack="True" Width="200"></asp:TextBox>
    <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />&nbsp;&nbsp;
    <br /><br />
    <div id="ClaimGA" runat="server">
        <div style="border: 1px solid #e3e3e3; align-content:flex-start">
            <asp:GridView ID ="gvInvoice" runat="server" AutoGenerateColumns="false" 
                OnRowEditing="gvInvoice_RowEditing" 
                CellPadding="1"
                ForeColor="#333333"
                GridLines="None" OnPageIndexChanging="gvInvoice_PageIndexChanging"  OnRowCancelingEdit="gvInvoice_RowCancelingEdit" 
                OnRowUpdating="gvInvoice_RowUpdating" OnRowDataBound="gvInvoice_RowDataBound" OnRowCommand="gvInvoice_RowCommand"
                AllowPaging="true" AllowCustomPaging="true">
                <Columns>
                    <asp:TemplateField HeaderText="No SPD" ItemStyle-VerticalAlign="Top" HeaderStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:LinkButton Text='<%# Eval("NomorSDP") %>' ID="noSPD" runat="server" CommandName="Select" CommandArgument="<%# Container.DataItemIndex %>" />
                        </ItemTemplate>
                        </asp:TemplateField>
                    <asp:TemplateField HeaderText="NamaLengkap" Visible="true" HeaderStyle-VerticalAlign="Top" ItemStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="namaLengkap"  runat="server" Text='<%# Eval("namaLengkap") %> '></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>                   
                    <%--<asp:TemplateField HeaderText="NRP" ItemStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="NRP" runat="server" Text='<%# Eval("nrp") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="TglBerangkat" ItemStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="TglBerangkat" runat="server" Text='<%# Eval("tglBerangkat","{0:M/d/yyyy}")%>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="TglKembali" ItemStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="TglKembali" runat="server" Text='<%# Eval("tglKembali","{0:M/d/yyyy}") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>--%>
                    <asp:TemplateField HeaderText="Tgl Issued" ItemStyle-VerticalAlign="Top" HeaderStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Eval("TanggalIssuedA","{0:yyyy/MM/dd}") %>'></asp:Label><br />
                            <asp:Label ID="Label2" runat="server" Text='<%# Eval("TanggalIssuedB","{0:yyyy/MM/dd}") %>'></asp:Label><br />
                            <asp:Label ID="Label3" runat="server" Text='<%# Eval("TanggalIssuedC","{0:yyyy/MM/dd}") %>'></asp:Label><br />
                            <asp:Label ID="Label4" runat="server" Text='<%# Eval("TanggalIssuedD","{0:yyyy/MM/dd}") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox1" 
                            AutoComplete="off" AutoCompleteType="None" Width="100px"
                            runat="server" CssClass="datepicker" Text='<%# Eval("TanggalIssuedA","{0:yyyy/MM/dd}") %>'></asp:TextBox>

                            <asp:TextBox  ID="TextBox2"
                            AutoComplete="off" AutoCompleteType="None" Width="100px"
                            runat="server" CssClass="datepicker" Text='<%# Eval("TanggalIssuedB","{0:yyyy/MM/dd}") %>'></asp:TextBox>

                            <asp:TextBox  ID="TextBox3" 
                            AutoComplete="off" AutoCompleteType="None" Width="100px"
                            runat="server" CssClass="datepicker"  Text='<%# Eval("TanggalIssuedC","{0:yyyy/MM/dd}") %>'></asp:TextBox>

                            <asp:TextBox  ID="TextBox4"
                            AutoComplete="off" AutoCompleteType="None" Width="100px"
                            runat="server" CssClass="datepicker" Text='<%# Eval("TanggalIssuedD","{0:yyyy/MM/dd}") %>'></asp:TextBox>

                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="NoInvoice Hotel" ItemStyle-VerticalAlign="Top" HeaderStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="Label5" runat="server" Text='<%# Eval("NoInvoiceHotelA") %>'></asp:Label><br />
                            
                            <asp:Label ID="Label6" runat="server" Text='<%# Eval("NoInvoiceHotelB") %>'></asp:Label><br />

                            <asp:Label ID="Label7" runat="server" Text='<%# Eval("NoInvoiceHotelC") %>'></asp:Label><br />

                            <asp:Label ID="Label8" runat="server" Text='<%# Eval("NoInvoiceHotelD") %>'></asp:Label>

                        </ItemTemplate>
                        <EditItemTemplate  >
                            <asp:TextBox ID="TextBox5" CssClass="text"  
                                runat="server" Width="100px" Text='<%# Eval("NoInvoiceHotelA") %>'></asp:TextBox>                            
                            
                            <asp:TextBox ID="TextBox6" CssClass="text"
                                runat="server" Width="100px" Text='<%# Eval("NoInvoiceHotelB") %>'></asp:TextBox>

                            <asp:TextBox ID="TextBox7"  CssClass="text"
                                runat="server" Width="100px" Text='<%# Eval("NoInvoiceHotelC") %>'></asp:TextBox>
                            
                            <asp:TextBox ID="TextBox8"  CssClass="text"
                                runat="server" Width="100px" Text='<%# Eval("NoInvoiceHotelD") %>'></asp:TextBox>

                            <%--<asp:RequiredFieldValidator
                                    ID="rvInvoiceHotel" runat="server"  
                                    ControlToValidate="txtNoInvoiceHotel" ForeColor="Red"
                                     ErrorMessage="Required" Display="Dynamic"></asp:RequiredFieldValidator>
                   
                            <asp:RequiredFieldValidator
                                    ID="RequiredFieldValidator1" runat="server"  
                                    ControlToValidate="TextBox1" ForeColor="Red"
                                     ErrorMessage="Required" Display="Dynamic"></asp:RequiredFieldValidator>--%>
                        </EditItemTemplate>
                     
                    </asp:TemplateField >
                    <asp:TemplateField HeaderText="Tgl Invoice Hotel" ItemStyle-VerticalAlign="Top" HeaderStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="Label9" runat="server" Text='<%# Eval("TglInvoiceHotelA","{0:yyyy/MM/dd}") %>'></asp:Label><br />
                            
                            <asp:Label ID="Label10" runat="server" Text='<%# Eval("TglInvoiceHotelB","{0:yyyy/MM/dd}") %>'></asp:Label><br />

                            <asp:Label ID="Label11" runat="server" Text='<%# Eval("TglInvoiceHotelC","{0:yyyy/MM/dd}") %>'></asp:Label><br />

                            <asp:Label ID="Label12" runat="server" Text='<%# Eval("TglInvoiceHotelD","{0:yyyy/MM/dd}") %>'></asp:Label>

                        </ItemTemplate>
                        
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox9"
                            AutoComplete="off" AutoCompleteType="None" Width="100px" Text='<%# Eval("TglInvoiceHotelA","{0:yyyy/MM/dd}") %>'
                            runat="server" CssClass="datepicker"></asp:TextBox>

                            <asp:TextBox ID="TextBox10"
                            AutoComplete="off" AutoCompleteType="None" Width="100px" Text='<%# Eval("TglInvoiceHotelB","{0:yyyy/MM/dd}") %>'
                            runat="server" CssClass="datepicker"></asp:TextBox>

                            <asp:TextBox ID="TextBox11"
                            AutoComplete="off" AutoCompleteType="None" Width="100px" Text='<%# Eval("TglInvoiceHotelC","{0:yyyy/MM/dd}") %>'
                            runat="server" CssClass="datepicker"></asp:TextBox>

                            <asp:TextBox ID="TextBox12"
                            AutoComplete="off" AutoCompleteType="None" Width="100px" Text='<%# Eval("TglInvoiceHotelD","{0:yyyy/MM/dd}") %>'
                            runat="server" CssClass="datepicker"></asp:TextBox>

                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="HargaHotel" ItemStyle-VerticalAlign="Top" HeaderStyle-VerticalAlign="Top"  >
                        <ItemTemplate>
                            <asp:Label ID="Label13" runat="server" Text='<%# Eval("HargaHotelA","{0:#,###}") %>'></asp:Label><br />
                            <asp:Label ID="Label14" runat="server" Text='<%# Eval("HargaHotelB","{0:#,###}") %>'></asp:Label><br />
                            <asp:Label ID="Label15" runat="server" Text='<%# Eval("HargaHotelC","{0:#,###}") %>'></asp:Label><br />
                            <asp:Label ID="Label16" runat="server" Text='<%# Eval("HargaHotelD","{0:#,###}") %>'></asp:Label><br />
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox13" CssClass="text" onkeydown="return(!(event.keyCode>=65 && event.keyCode <=95) && event.keyCode !=32);" Width="100px" runat="server" Text='<%# Eval("HargaHotelA") %>'></asp:TextBox>
                            <asp:TextBox ID="TextBox14" CssClass="text" onkeydown="return(!(event.keyCode>=65 && event.keyCode <=95) && event.keyCode !=32);" Width="100px" runat="server" Text='<%# Eval("HargaHotelB") %>'></asp:TextBox>
                            <asp:TextBox ID="TextBox15" CssClass="text" onkeydown="return(!(event.keyCode>=65 && event.keyCode <=95) && event.keyCode !=32);" Width="100px" runat="server" Text='<%# Eval("HargaHotelC") %>'></asp:TextBox>
                            <asp:TextBox ID="TextBox16" CssClass="text" onkeydown="return(!(event.keyCode>=65 && event.keyCode <=95) && event.keyCode !=32);" Width="100px" runat="server" Text='<%# Eval("HargaHotelD") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="NoInvoice Tiket" ItemStyle-VerticalAlign="Top" HeaderStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="Label17" runat="server" Text='<%# Eval("NoInvoiceTiketA") %>'></asp:Label><br />

                            <asp:Label ID="Label18" runat="server" Text='<%# Eval("NoInvoiceTiketB") %>'></asp:Label><br />

                            <asp:Label ID="Label19" runat="server" Text='<%# Eval("NoInvoiceTiketC") %>'></asp:Label><br />

                            <asp:Label ID="Label20" runat="server" Text='<%# Eval("NoInvoiceTiketC") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox17" CssClass="text" Width="100px" runat="server" Text='<%# Eval("NoInvoiceTiketA") %>'></asp:TextBox>

                            <asp:TextBox ID="TextBox18" CssClass="text" Width="100px" runat="server" Text='<%# Eval("NoInvoiceTiketB") %>'></asp:TextBox>

                            <asp:TextBox ID="TextBox19" CssClass="text" Width="100px" runat="server" Text='<%# Eval("NoInvoiceTiketC") %>'></asp:TextBox>

                            <asp:TextBox ID="TextBox20" CssClass="text" Width="100px" runat="server" Text='<%# Eval("NoInvoiceTiketC") %>'></asp:TextBox>
<%--                            <asp:RequiredFieldValidator
                                    ID="rvInvoiceTiket" runat="server" ForeColor="Red" 
                                    ControlToValidate="txtNoInvoiceTiket"
                                     ErrorMessage="Required" Display="Dynamic"></asp:RequiredFieldValidator>--%>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Tgl Invoice Tiket" ItemStyle-VerticalAlign="Top" HeaderStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="Label21" runat="server" Text='<%# Eval("TglInvoiceTiketA","{0:yyyy/MM/dd}") %>'></asp:Label><br />
                            <asp:Label ID="Label22" runat="server" Text='<%# Eval("TglInvoiceTiketB","{0:yyyy/MM/dd}") %>'></asp:Label><br />
                            <asp:Label ID="Label23" runat="server" Text='<%# Eval("TglInvoiceTiketC","{0:yyyy/MM/dd}") %>'></asp:Label><br />
                            <asp:Label ID="Label24" runat="server" Text='<%# Eval("TglInvoiceTiketC","{0:yyyy/MM/dd}") %>'></asp:Label>

                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox21"
                            AutoComplete="off" AutoCompleteType="None" Width="120px"
                            runat="server" CssClass="datepicker" Text='<%# Eval("TglInvoiceTiketA","{0:yyyy/MM/dd}") %>'></asp:TextBox>

                            <asp:TextBox ID="TextBox22"
                            AutoComplete="off" AutoCompleteType="None" Width="120px"
                            runat="server" CssClass="datepicker" Text='<%# Eval("TglInvoiceTiketB","{0:yyyy/MM/dd}") %>'></asp:TextBox>

                            <asp:TextBox ID="TextBox23"
                            AutoComplete="off" AutoCompleteType="None" Width="120px"
                            runat="server" CssClass="datepicker" Text='<%# Eval("TglInvoiceTiketC","{0:yyyy/MM/dd}") %>'></asp:TextBox>

                            <asp:TextBox ID="TextBox24"
                            AutoComplete="off" AutoCompleteType="None" Width="120px"
                            runat="server" CssClass="datepicker" Text='<%# Eval("TglInvoiceTiketC","{0:yyyy/MM/dd}") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="HargaTiket" ItemStyle-VerticalAlign="Top" HeaderStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="Label25" runat="server" Text='<%# Eval("HargaTiketA","{0:#,###}") %>'></asp:Label><br />
                            <asp:Label ID="Label26" runat="server" Text='<%# Eval("HargaTiketB","{0:#,###}") %>'></asp:Label><br />
                            <asp:Label ID="Label27" runat="server" Text='<%# Eval("HargaTiketC","{0:#,###}") %>'></asp:Label><br />
                            <asp:Label ID="Label28" runat="server" Text='<%# Eval("HargaTiketD","{0:#,###}") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox25" CssClass="text" onkeydown="return(!(event.keyCode>=65 && event.keyCode <=95) && event.keyCode !=32);" runat="server" Width="100px" Text='<%# Eval("HargaTiketA") %>'></asp:TextBox>
                            <asp:TextBox ID="TextBox26" CssClass="text" onkeydown="return(!(event.keyCode>=65 && event.keyCode <=95) && event.keyCode !=32);" runat="server" Width="100px" Text='<%# Eval("HargaTiketB") %>'></asp:TextBox>
                            <asp:TextBox ID="TextBox27" CssClass="text" onkeydown="return(!(event.keyCode>=65 && event.keyCode <=95) && event.keyCode !=32);" runat="server" Width="100px" Text='<%# Eval("HargaTiketC") %>'></asp:TextBox>
                            <asp:TextBox ID="TextBox28" CssClass="text" onkeydown="return(!(event.keyCode>=65 && event.keyCode <=95) && event.keyCode !=32);" runat="server" Width="100px" Text='<%# Eval("HargaTiketD") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="TglTerimaInv" ItemStyle-VerticalAlign="Top" HeaderStyle-VerticalAlign="Top">
                        <ItemTemplate>
                            <asp:Label ID="Label29" runat="server" Text='<%# Eval("TglTerimaInvoiceA","{0:yyyy/MM/dd hh:mm}") %>'></asp:Label>
                            <br />
                            <asp:Label ID="Label30" runat="server" Text='<%# Eval("TglTerimaInvoiceB","{0:yyyy/MM/dd hh:mm}") %>'></asp:Label>
                            <br />
                            <asp:Label ID="Label31" runat="server" Text='<%# Eval("TglTerimaInvoiceC","{0:yyyy/MM/dd hh:mm}") %>'></asp:Label>
                            <br />
                            <asp:Label ID="Label32" runat="server" Text='<%# Eval("TglTerimaInvoiceD","{0:yyyy/MM/dd hh:mm}") %>'></asp:Label>
                           
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox29" AutoComplete="off" AutoCompleteType="None" AutoPostBack="true" CssClass="datepicker" runat="server" Width="90px" Text='<%# Eval("TglTerimaInvoiceA","{0:yyyy/MM/dd}") %>' OnTextChanged="TextBox29_TextChanged"></asp:TextBox>
                            <asp:TextBox ID="TextBox30" AutoComplete="off" AutoCompleteType="None" AutoPostBack="true" CssClass="datepicker" runat="server" Width="90px" Text='<%# Eval("TglTerimaInvoiceB","{0:yyyy/MM/dd}") %>' OnTextChanged="TextBox30_TextChanged"></asp:TextBox>
                            <asp:TextBox ID="TextBox31" AutoComplete="off" AutoCompleteType="None" AutoPostBack="true" CssClass="datepicker" runat="server" Width="90px" Text='<%# Eval("TglTerimaInvoiceC","{0:yyyy/MM/dd}") %>' OnTextChanged="TextBox31_TextChanged"></asp:TextBox>
                            <asp:TextBox ID="TextBox32" AutoComplete="off" AutoCompleteType="None" AutoPostBack="true" CssClass="datepicker" runat="server" Width="90px" Text='<%# Eval("TglTerimaInvoiceD","{0:yyyy/MM/dd}") %>' OnTextChanged="TextBox32_TextChanged"></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField ItemStyle-VerticalAlign="Top" HeaderText="Jam" HeaderStyle-VerticalAlign="Top">
                        <ItemTemplate></ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox33" AutoComplete="off" AutoCompleteType="None" CssClass="timepicker" runat="server" Width="40px" Text='<%# Eval("TglTerimaInvoiceA","{0:hh:mm}") %>'></asp:TextBox>
                            <asp:TextBox ID="TextBox34" AutoComplete="off" AutoCompleteType="None" CssClass="timepicker" runat="server" Width="40px" Text='<%# Eval("TglTerimaInvoiceB","{0:hh:mm}") %>'></asp:TextBox>
                            <asp:TextBox ID="TextBox35" AutoComplete="off" AutoCompleteType="None" CssClass="timepicker" runat="server" Width="40px" Text='<%# Eval("TglTerimaInvoiceC","{0:hh:mm}") %>'></asp:TextBox>
                            <asp:TextBox ID="TextBox36" AutoComplete="off" AutoCompleteType="None" CssClass="timepicker" runat="server" Width="40px" Text='<%# Eval("TglTerimaInvoiceD","{0:hh:mm}") %>'></asp:TextBox>                    
                        </EditItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:CommandField ButtonType="Image" ShowEditButton="true"  
                        UpdateImageUrl="Style/icons8-save-20.png"
                        ItemStyle-Width="80px" EditText="Edit" ItemStyle-Wrap="true" ItemStyle-BorderWidth="0"
                        EditImageUrl="Style/icons8-edit-20.png" ItemStyle-HorizontalAlign="Center" FooterStyle-VerticalAlign="Middle"
                        CancelImageUrl="Style/icons8-go-back-20.png" ItemStyle-VerticalAlign="Top" />

                </Columns>
                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Left" CssClass="GridPager" />
                <RowStyle BackColor="#E3EAEB" />
                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />                
            </asp:GridView>
            <asp:Panel runat="server" ID="panelMessage">
                <table style="align-content:center; border-style:groove; background-color:white" width="300px" height="150px" >
                    <tr>
                        <td align="center">
                            <asp:Label runat="server" ID="labelA"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td align="center">
                            <asp:Button ID="btnCancel" BorderStyle="Groove" runat="server" Text="Close" OnClientClick = "return Hidepopup()"/>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:LinkButton ID="lnkFake" runat="server"></asp:LinkButton>
            <ajax:ModalPopupExtender ID="popupMessage"
                 runat="server"
                 PopupControlID="panelMessage" TargetControlID="lnkFake"
                 BackgroundCssClass=""></ajax:ModalPopupExtender>
        </div>
    </div>
</asp:Content>

