using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Text;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using System;

namespace Sitecore.Support.Shell.Applications.ContentManager.Dialogs.SetPublishing
{
    public class SetPublishingForm : Sitecore.Shell.Applications.ContentManager.Dialogs.SetPublishing.SetPublishingForm
    {
        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            Item itemFromQueryString = UIUtil.GetItemFromQueryString(Context.ContentDatabase);
            Error.AssertItemFound(itemFromQueryString);
            ListString listString = new ListString();
            using (new StatisticDisabler(StatisticDisablerState.ForItemsWithoutVersionOnly))
            {
                itemFromQueryString.Editing.BeginEdit();
                itemFromQueryString.Publishing.NeverPublish = !NeverPublish.Checked;

                #region Sitecore.Support.319710 - Fixes the 'ITEM' tab
                //Old Code:
                //itemFromQueryString.Publishing.PublishDate = DateUtil.ParseDateTime(Publish.Value, DateTimeOffset.MinValue.UtcDateTime).ToUniversalTime();
                //itemFromQueryString.Publishing.UnpublishDate = DateUtil.ParseDateTime(Unpublish.Value, DateTimeOffset.MaxValue.UtcDateTime).ToUniversalTime();

                //New Code:
                DateUtil.ToUniversalTime(itemFromQueryString.Publishing.PublishDate = DateUtil.ParseDateTime(Publish.Value, DateTimeOffset.MinValue.UtcDateTime));
                DateUtil.ToUniversalTime(itemFromQueryString.Publishing.UnpublishDate = DateUtil.ParseDateTime(Unpublish.Value, DateTimeOffset.MaxValue.UtcDateTime));
                #endregion

                foreach (string key in Context.ClientPage.ClientRequest.Form.Keys)
                {
                    if (key != null && key.StartsWith("pb_", StringComparison.InvariantCulture))
                    {
                        string value = ShortID.Decode(StringUtil.Mid(key, 3));
                        listString.Add(value);
                    }
                }
                itemFromQueryString[FieldIDs.PublishingTargets] = listString.ToString();
                itemFromQueryString.Editing.EndEdit();
            }
            Log.Audit(this, "Set publishing targets: {0}, targets: {1}", AuditFormatter.FormatItem(itemFromQueryString), listString.ToString());
            foreach (string key2 in Context.ClientPage.ClientRequest.Form.Keys)
            {
                if (key2 != null && key2.StartsWith("pb_", StringComparison.InvariantCulture))
                {
                    string value2 = ShortID.Decode(StringUtil.Mid(key2, 3));
                    listString.Add(value2);
                }
            }
            Item[] versions = itemFromQueryString.Versions.GetVersions();
            foreach (Item item in versions)
            {
                bool flag = StringUtil.GetString(Context.ClientPage.ClientRequest.Form["hide_" + item.Version.Number]).Length <= 0;
                DateTimePicker dateTimePicker = Versions.FindControl("validfrom_" + item.Version.Number) as DateTimePicker;
                DateTimePicker obj = Versions.FindControl("validto_" + item.Version.Number) as DateTimePicker;
                Assert.IsNotNull(dateTimePicker, "Version valid from datetime picker");
                Assert.IsNotNull(obj, "Version valid to datetime picker");

                #region Sitecore.Support.319710 - Fixes the 'VERSION' tab
                //Old Code:
                //DateTime dateTime = DateUtil.IsoDateToDateTime(dateTimePicker.Value, DateTimeOffset.MinValue.UtcDateTime).ToUniversalTime();
                //DateTime dateTime2 = DateUtil.IsoDateToDateTime(obj.Value, DateTimeOffset.MaxValue.UtcDateTime).ToUniversalTime();

                //New Code:
                DateTime dateTime = DateUtil.ToUniversalTime(DateUtil.IsoDateToDateTime(dateTimePicker.Value, DateTimeOffset.MinValue.UtcDateTime));
                DateTime dateTime2 = DateUtil.ToUniversalTime(DateUtil.IsoDateToDateTime(obj.Value, DateTimeOffset.MaxValue.UtcDateTime));
                #endregion

                if (flag != item.Publishing.HideVersion || DateUtil.CompareDatesIgnoringSeconds(dateTime, item.Publishing.ValidFrom) != 0 || DateUtil.CompareDatesIgnoringSeconds(dateTime2, item.Publishing.ValidTo) != 0)
                {
                    item.Editing.BeginEdit();
                    item.Publishing.ValidFrom = dateTime;
                    item.Publishing.ValidTo = dateTime2;
                    item.Publishing.HideVersion = flag;
                    item.Editing.EndEdit();
                    Log.Audit(this, "Set publishing valid: {0}, from: {1}, to:{2}, hide: {3}", AuditFormatter.FormatItem(item), dateTime.ToString(), dateTime2.ToString(), MainUtil.BoolToString(flag));
                }
            }
            SheerResponse.SetDialogValue("yes");

            #region Sitecore.Support.319710 - Need to call code from 'Grandparent' class
            //Old Code: 
            //base.OnOK(sender, args);

            //New Code:
            SheerResponse.CloseWindow();    // "base.base.OnOK(sender, args);"
            #endregion
        }
    }
}