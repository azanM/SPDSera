﻿<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPage.Master"
    CodeBehind="msUser.aspx.cs" Inherits="eSPD.msUser" %>

<%@ Register Assembly="DevExpress.Web.v14.1" Namespace="DevExpress.Web.ASPxEditors"
    TagPrefix="dx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table width="100%">
        <tr>
            <td>
                <asp:UpdatePanel ID="UpdatePanel2" runat="server" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <asp:Panel ID="pnlForm" runat="server">
                            <table width="auto" align="left">
                                <tr>
                                    <td>
                                        Role
                                    </td>
                                    <td>
                                        :
                                    </td>
                                    <td width="270">
                                        <asp:DropDownList ID="cmbCompanyName" runat="server" AutoPostBack="True" DataSourceID="ldCompanyName"
                                            DataTextField="namaRole" DataValueField="roleId" OnSelectedIndexChanged="cmbCompanyName_SelectedIndexChanged">
                                        </asp:DropDownList>
                                    </td>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" ControlToValidate="cmbCompanyName"
                                        ErrorMessage="*" Font-Bold="True" Font-Size="Small" SetFocusOnError="True" ValidationGroup="a"></asp:RequiredFieldValidator>
                                    <asp:LinqDataSource ID="ldCompanyName" runat="server" ContextTypeName="eSPD.dsSPDDataContext"
                                        EntityTypeName="" Select="new (id as roleId, namaRole as namaRole)" TableName="msRoles">
                                    </asp:LinqDataSource>
                                </tr>
                                <tr>
                                    <td>
                                        NRP
                                    </td>
                                    <td>
                                        :
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtCostDesc" runat="server" Width="300px" AutoPostBack="true" OnTextChanged="txtCostCenter_TextChanged"></asp:TextBox>
                                        <asp:Label ID="lblCostDesc" runat="server" ForeColor="#CC0000" Text="*"></asp:Label>
                                        <asp:RequiredFieldValidator ID="valCostDesc" runat="server" ErrorMessage="NRP Tidak boleh kosong"
                                            ValidationGroup="a" ControlToValidate="txtCostDesc" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                        <asp:HiddenField runat="server" ID="hfCostId" />
                                    </td>
                                </tr>
                                <tr runat="server" id="trCostCenter">
                                    <td>
                                        Nama Lengkap
                                    </td>
                                    <td>
                                        :
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtCostCenter" runat="server" Width="300px" AutoPostBack="true"
                                            Enabled="False"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2" align="right">
                                    </td>
                                    <td align="left">
                                        <asp:Button ID="btnSimpan" runat="server" Text="Simpan" OnClick="btnSimpan_Click"
                                            ValidationGroup="a" />
                                        &nbsp;
                                        <asp:Button ID="btnBatal" runat="server" Text="Batal" OnClick="btnBatal_Click" />
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="3">
                                        <asp:Label ID="notif" runat="server" Text="" Visible="true" ViewStateMode="Disabled"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnBatal" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnSimpan" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </td>
        </tr>
        <tr>
            <td align="left">
                <asp:UpdatePanel ID="UpdatePanel3" runat="server" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <asp:Button ID="btnTambah" runat="server" Text="Tambah" OnClick="btnTambah_Click" />
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
                        <asp:GridView ID="gvUser" runat="server" AutoGenerateColumns="False" CellPadding="4"
                            ForeColor="#333333" GridLines="None" AllowPaging="true" OnPageIndexChanging="gvCostCenter_PageIndexChanging">
                            <AlternatingRowStyle BackColor="White" />
                            <Columns>
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Label runat="server" Visible="false" ID="id" Text='<%# Eval("id") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="NRP" HeaderText="NRP" />
                                <asp:BoundField DataField="namaLengkap" HeaderText="Nama Lengkap" />
                                <asp:BoundField DataField="namaRole" HeaderText="Role" />
                                <asp:TemplateField HeaderText="Edit">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lbEdit" runat="server" OnClick="lbEdit_Click">Edit</asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Delete">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lbDelete" runat="server" OnClick="lbDelete_Click">Delete</asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
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
    </table>
</asp:Content>
