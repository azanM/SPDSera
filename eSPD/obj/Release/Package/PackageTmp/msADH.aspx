<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPage.Master" CodeBehind="msADH.aspx.cs" Inherits="eSPD.msADH" %>


<%@ Register assembly="DevExpress.Web.v14.1" namespace="DevExpress.Web.ASPxEditors" tagprefix="dx" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="asp" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table width="100%">
        
        <tr>
            <td align="left">
                <asp:UpdatePanel ID="UpdatePanel3" runat="server" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div id="Filter" runat="server">
                        Cost Center Code :
                            <asp:TextBox ID="txtFilter" runat="server" Width="150px"></asp:TextBox>
                            &nbsp;
                            <asp:Button ID="btnFilter" runat="server" Text="Search" OnClick="btnFilter_Click" />
                            &nbsp;
                        </div>
                        <asp:Button ID="btnTambah" runat="server" Text="TambahADH " OnClick="btnTambah_Click" />
                        <asp:HiddenField ID="hfmode" runat="server" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </td>
            <td>
            </td>
        </tr>
        <tr>
            <td align="left">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <asp:GridView ID="gvMasterADH" runat="server" AutoGenerateColumns="False" CellPadding="4"
                             OnRowUpdating="gvMasterADH_RowUpdating"
                             OnRowEditing="gvMasterADH_RowEditing"
                             OnRowDataBound="gvMasterADH_RowDataBound"
                             OnRowCancelingEdit="gvMasterADH_RowCancelingEdit"
                             OnRowDeleting="gvMasterADH_RowDeleting"
                            ForeColor="#333333" GridLines="None" AllowPaging="true" OnPageIndexChanging="gvMasterADH_PageIndexChanging">
                            <AlternatingRowStyle BackColor="White" />
                            <Columns>
                              <asp:TemplateField HeaderText="ID">
                                    <ItemTemplate>
                                        <asp:Label runat="server" ID="Id" Text='<%# Eval("id") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="NRP">
                                    <ItemTemplate>
                                        <asp:Label runat="server" ID="nrp" Text='<%# Eval("nrp") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox runat="server" ID="nrp" Text='<%# Eval("nrp") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Cost Center">
                                    <ItemTemplate>
                                        <asp:Label runat="server" ID="costCenter" Text='<%# Eval("costDesc") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:DropDownList runat="server" ID="ddlCostCenter"></asp:DropDownList>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                    <asp:CommandField ButtonType="Image" ShowEditButton="true" ShowDeleteButton="true"  
                        UpdateImageUrl="Style/icons8-save-20.png"
                        DeleteImageUrl="Style/icons8-cancel-20.png"
                        ItemStyle-Width="80px" EditText="Edit" ItemStyle-Wrap="true" ItemStyle-BorderWidth="0"
                        EditImageUrl="Style/icons8-edit-20.png" ItemStyle-HorizontalAlign="Center" FooterStyle-VerticalAlign="Middle"
                        CancelImageUrl="Style/icons8-go-back-20.png" ItemStyle-VerticalAlign="Top" />

                                <%--<asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Label runat="server" Visible="false" ID="id" Text='<%# Eval("id") %>' />
                        
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="nrp" HeaderText="NRP" />
                                <asp:BoundField Visible="false" DataField="nama" HeaderText="Nama Lengkap" />
                                <asp:BoundField DataField="costDesc" HeaderText="Cost Description" />
                                <asp:TemplateField HeaderText="Edit">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lbEdit" runat="server" OnClick="lbEdit_Click">Edit</asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Delete">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lbDelete" runat="server" 
                                            OnClick="lbDelete_Click"
                                            OnClientClick ="if (!confirm('Are you sure?')) {return false;} else {return true;}">Delete</asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>--%>

                            </Columns>
                            <EditRowStyle BackColor="#7C6F57" />
                            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                            <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                            <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                            <RowStyle BackColor="#E3EAEB" />
                            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                            <SortedAscendingCellStyle BackColor="#F8FAFA" />
                            <SortedAscendingHeaderStyle BackColor="#246B61" />
                            <SortedDescendingCellStyle BackColor="#D4DFE1" />
                            <SortedDescendingHeaderStyle BackColor="#15524A" />
                        </asp:GridView>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnSimpan" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </td>
        </tr>
        <tr>
            <td>
                <asp:UpdatePanel ID="UpdatePanel2" runat="server" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <asp:Panel ID="inputForm" runat="server">
                            <table width="auto" align="left">
                                <tr>
                                    <td>Cost Center</td>
                                    <td>:</td>
                                    <td>
                                        <asp:DropDownList ID="costCenterList" runat="server" Width="390px"
                                            DataTextField="costDesc" DataValueField="costId" OnSelectedIndexChanged="costCenter_SelectedIndexChanged">
                                        </asp:DropDownList>
                                    </td>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" ControlToValidate="costCenterList"
                                        ErrorMessage="*" Font-Bold="True" Font-Size="Small" SetFocusOnError="True" ValidationGroup="a"></asp:RequiredFieldValidator>
                                </tr>
                                <tr>
                                    <td>NRP</td>
                                    <td>:</td>
                                    <td>
                                        <asp:TextBox ID="txtNRP" runat="server" Width="300px"></asp:TextBox>
                                        <asp:Label ID="lblNRP" runat="server" ForeColor="#CC0000" Text="*"></asp:Label>
                                        <asp:RequiredFieldValidator ID="valCostDesc" runat="server" ErrorMessage="NRP Tidak boleh kosong"
                                            ValidationGroup="a" ControlToValidate="txtNRP" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                        <asp:HiddenField runat="server" ID="hfmasterADHId" />
                                    </td>
                                </tr>
                                <tr runat="server" id="trNamaLengkap" visible="false">
                                    <td>Nama Lengkap</td>
                                    <td>:</td>
                                    <td>
                                        <asp:TextBox ID="txtNamaLengkap" runat="server" Width="300px" AutoPostBack="true"
                                            Enabled="true"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2" align="right">
                                    </td>
                                    <td align="left">
                                        <asp:Button ID="btnSimpan" runat="server" Text="Simpan" OnClick="btnSimpan_Click"
                                            ValidationGroup="a" />
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="3">
                                        <asp:Label ID="notif" runat="server" Text="" ForeColor="Red" Visible="true" ViewStateMode="Disabled"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnSimpan" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </td>
        </tr>
    </table>
</asp:Content>