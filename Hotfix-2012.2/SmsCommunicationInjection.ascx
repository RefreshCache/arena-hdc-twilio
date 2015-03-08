<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmsCommunicationInjection.ascx.cs"
    CodeBehind="SmsCommunicationInjection.ascx.cs" Inherits="Arena.Custom.HDC.Twilio.SmsCommunicationInjection" %>
<script type="text/javascript">

    $(document).ready(function () {

        // Hide the from name field.

        var from = $('input[id$="_tbFrom"]').parent().parent();
        $(from).hide();

        // Hide the from number field.

        var tbEmail = $('input[id$="_tbEmail"]');
        $(tbEmail).hide();

        // Add a new drop down for the from number.

        var ddl = $('<select><option value=""></option></select>');
<asp:Literal ID="ltSelect" runat="server" />
        $(ddl).insertBefore(tbEmail);

        // Update the hidden field when the drop down changes.

        $(ddl).change(function() {
            $('input[id$="_tbEmail"]').val($(this).val());

        var opt = $(ddl).find('option[value="' + $(tbEmail).val() + '"]');
        if (opt.length > 0)
            $(ddl).val(opt.val());

        else
            $(ddl).val('');

        // If there is only 1 real phone number, select it.

        var children = $(ddl).children();
        if (children.length == 2)
            $(ddl).val($(ddl).children(':last').val());

        });

    });

</script>
