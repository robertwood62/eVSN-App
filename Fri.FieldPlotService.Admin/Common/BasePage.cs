using Fri.FieldPlotService.Admin.Dialogs;
using Fri.FieldPlotService.Admin.Resources;
using Fri.FieldPlotService.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

namespace Fri.FieldPlotService.Admin.Common
{
    /// <summary>
    /// Base page for all pages in the system.
    /// </summary>
    public class BasePage : OwningComponentBase
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Access to the navigation manager.
        /// </summary>
        [Inject]
        public NavigationManager Navigation { get; set; }
        
        /// <summary>
        /// Access to the admin context.
        /// </summary>
        [Inject]
        public AdminContext AdminContext { get; set; }

        /// <summary>
        /// Access to the snackbar service.
        /// </summary>
        [Inject]
        public ISnackbar Snackbar { get; set; }

        /// <summary>
        /// Access to the Dialog Service into the page.
        /// </summary>
        [Inject]
        public IDialogService Dialog { get; set; }

        /// <summary>
        /// Access to the JavaScript Runtime.
        /// </summary>
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Provides access to the token provider
        /// </summary>
        [Inject]
        IAccessTokenProvider? TokenProvider { get; set; }

        /// <summary>
        /// Access to the localizer into the page.
        /// </summary>
        [Inject]
        public IStringLocalizer<Strings> Localizer { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Every page can have a list of bread crumbs.
        /// </summary>
        protected List<BreadcrumbItem> Breadcrumbs = new List<BreadcrumbItem> { new BreadcrumbItem("(missing)", "#") };

        private bool isBusy = true;

        /// <summary>
        /// Sets the page to busy (or not busy).
        /// </summary>
        /// <returns></returns>
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                // If the state changes, update the page.
                if (isBusy != value)
                {
                    isBusy = value;
                    UpdatePage();
                }
            }
        }

        public bool IsInitialized { get; set; } = false;

        /// <summary>
        /// Overrides the initialize which sets the current security token.
        /// </summary>
        /// <returns></returns>
        protected async override Task OnInitializedAsync()
        {
            // Update the current bearer token value for the backend API.
            if(FieldPlotServiceApiClient.BearerToken == null)
            {
                if (TokenProvider != null)
                {
                    var accessTokenResult = await TokenProvider.RequestAccessToken();
                    if (accessTokenResult.TryGetToken(out var token))
                    {
                        FieldPlotServiceApiClient.BearerToken = token.Value;
                    }
                }
            }

            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Updates the page contents (renders the page after state changes)
        /// </summary>
        public void UpdatePage()
        {
            StateHasChanged();
        }

        /// <summary>
        /// Displays an exception error message on the page.
        /// </summary>
        /// <param name="ex"></param>
        public void ShowError(Exception ex)
        {
            IsBusy = false;

            // Determine what kind of exception was generated.
            string message;
            string? details;

            if (ex is ApiException<ApiErrorModel> e)
            {
                message = string.Format(Localizer["Error_Code"], e.Result.ErrorCode) + Localizer[$"Api_Error_{e.Result.ErrorCode}"];
                details = e.Result.Message;
            }
            else if (ex is ApiException apiEx)
            {
                message = string.Format(Localizer["Error_Http"], apiEx.StatusCode) + Localizer[$"Api_Http_{apiEx.StatusCode}"];
                details = ex.Message;
            }
            else
            {
                message = ex.Message;
                details = ex.ToString();
            }

            var options = new DialogOptions { CloseOnEscapeKey = true };
            var parameters = new DialogParameters
            {
                { "Message", message },
                { "Details", details }
            };

            Dialog?.Show<ErrorDialog>(Localizer["Dialogs_ErrorDialog_Title"] ?? "Error", parameters, options);
        }

        /// <summary>
        /// Displays a simple confirmation message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> ConfirmAsync(string message)
        {
            bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", message);
            return confirmed;
        }

        /// <summary>
        /// Displays an alert message on the page.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title">Null for delete</param>
        /// <param name="confirmAction">Action to perform if the dialog is confirmed.</param>
        /// <param name="confirmText">Default to 'Ok'</param>
        /// <param name="cancelText">Defaults to 'Cancel'</param>
        public void ShowPrompt(string message, string? title, Func<Task> confirmAction, string? confirmText = null, string? cancelText = null)
        {
            IsBusy = false;

            //// Use the default title based on the alert type.
            //if (string.IsNullOrWhiteSpace(title))
            //{
            //    title = adminContext.Localizer.GetString($"ErrorDialog_{AlertType.Prompt}Title");
            //}

            //var alert = new AlertDialogInfo
            //{
            //    Type = AlertType.Prompt,
            //    Title = title,
            //    Message = message,
            //    Details = null,
            //    OnConfirm = confirmAction,
            //    CloseButton = cancelText ?? adminContext.Localizer.GetString("Button_Cancel"),
            //    ConfirmButton = confirmText ?? adminContext.Localizer.GetString("Button_Ok"),
            //};

            //var options = new DialogOptions { CloseOnEscapeKey = true };
            //var parameters = new DialogParameters
            //{
            //    { "Alert", alert }
            //};

            //dialogService.Show<PromptDialog>(alert.Title, parameters, options);
        }

    }

}
