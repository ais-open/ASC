using System.Web;
using System.Web.Optimization;

namespace AzureServiceCatalog.Web
{
    public static class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/vendorjs").Include(
                    "~/lib/ace-builds/src-min-noconflict/ace.js",
                    "~/lib/angular/angular.js",
                    "~/lib/adal-angular/lib/adal.js",
                    "~/lib/adal-angular/lib/adal-angular.js",
                    "~/lib/angular-sanitize/angular-sanitize.js",
                    "~/lib/angular-animate/angular-animate.js",
                    "~/lib/angular-bootstrap/ui-bootstrap-tpls.js",
                    "~/lib/angular-toastr/dist/angular-toastr.tpls.js",
                    "~/lib/angular-ui-router/release/angular-ui-router.js",
                    "~/lib/angular-ui-select/dist/select.js",
                    "~/lib/angular-ui-grid/ui-grid.js",
                    "~/lib/lodash/lodash.js",
                    "~/lib/spin.js/spin.js",
                    "~/lib/moment/moment.js",
                    "~/lib/angular-ui-ace/ui-ace.js",
                    "~/lib/angular-truncate/src/truncate.js",
                    "~/lib/angular-toggle-switch/angular-toggle-switch.js",
                    "~/lib/d3/d3.js",
                    "~/lib/nvd3/build/nv.d3.js",
                    "~/lib/angular-nvd3/dist/angular-nvd3.js",
                    "~/lib/angular-ui-tree/dist/angular-ui-tree.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/appjavascript").Include(
                    "~/app/app.js",
                    "~/app/shared/state-watcher.service.js",
                    "~/app/shared/resource-group-selector.controller.js",
                    "~/app/shared/json-modal.controller.js",
                    "~/app/shared/error-dialog-box.controller.js",
                    "~/app/shared/edit-field-modal.controller.js",
                    "~/app/shared/constants.js",
                    "~/app/shared/confirm-modal.controller.js",
                    "~/app/shared/authorization-checker.service.js",
                    "~/app/services/task-manager.service.js",
                    "~/app/services/product-list.service.js",
                    "~/app/services/dialogs.service.js",
                    "~/app/services/app-storage-service.js",
                    "~/app/services/app-spinner.service.js",
                    "~/app/services/acs-api.service.js",
                    "~/app/services/Bill-Info.service.js",
                    "~/app/security/security.controller.js",
                    "~/app/security/security-initial-data.service.js",
                    "~/app/product-details/product-details.controller.js",
                    "~/app/product-deployment/product-deployment.controller.js",
                    "~/app/product-deployment/product-deployment-initial-data.service.js",
                    "~/app/product-deployment/new-resource-group-modal.controller.js",
                    "~/app/product-deployment/new-resource-group-initial-data.service.js",
                    "~/app/resource-groups/resource-groups.controller.js",
                    "~/app/home/blueprints-home.controller.js",
                    "~/app/home/home.controller.js",
                    "~/app/manage-products/quick-starts-modal.controller.js",
                    "~/app/manage-products/product-list.controller.js",
                    "~/app/manage-products/edit-product.controller.js",
                    "~/app/manage-products/add-product-designer.controller.js",
                    "~/app/policy/policy-list.controller.js",
                    "~/app/policy/edit-policy.controller.js",
                    "~/app/policy-assignments/policy-assignment-list.controller.js",
                    "~/app/policy-assignments/edit-policy-assignment.controller.js",
                    "~/app/policy-assignments/edit-policy-assignment-initial-data.service.js",
                    "~/app/layout/tasks-pane.controller.js",
                    "~/app/layout/shell.controller.js",
                    "~/app/layout/deployment-operations.controller.js",
                    "~/app/identity/select-group.controller.js",
                    "~/app/identity/manage-user-groups.controller.js",
                    "~/app/identity/manage-host-subscription.controller.js",
                    "~/app/identity/manage-enrolled-subscriptions.controller.js",
                    "~/app/identity/login.controller.js",
                    "~/app/identity/deactivate-tenant.controller.js",
                    "~/app/identity/activation.controller.js",
                    "~/app/identity/activation-login.controller.js",
                    "~/app/directives/spinner.directive.js",
                    "~/app/directives/resource-group-selector.component.js",
                    "~/app/directives/lvl-uuid.js",
                    "~/app/directives/lvl-drag-drop.directive.js",
                    "~/app/directives/file-reader.directive.js",
                    "~/app/directives/dontPropagate.directive.js",
                    "~/app/deployments/deployments.controller.js",
                    "~/app/deployments/deployment.controller.js",
                    "~/app/dashboard/dashboard.controller.js",
                    "~/app/blueprint/assign-blueprint.controller.js",
                    "~/app/blueprint/blueprint-assignments.controller.js",
                    "~/app/blueprint/view-blueprint-details-modal.controller.js",
                    "~/app/blueprint/view-blueprint-details-initial-data.service.js",
                    "~/app/app.run.js",
                    "~/app/app.config.js",
                    "~/app/support/feedback.controller.js"));

            //BundleTable.EnableOptimizations = true;
        }
    }
}
