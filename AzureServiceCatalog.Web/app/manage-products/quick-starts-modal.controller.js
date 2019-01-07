(function () {
    'use strict';

    angular.module('ascApp').controller('QuickStartsModalCtrl', QuickStartsModalCtrl);

    QuickStartsModalCtrl.$inject = ['$uibModalInstance', 'initialData', 'uiGridConstants'];

    /* @ngInject */
    function QuickStartsModalCtrl($uibModalInstance, initialData, uiGridConstants) {
        /* jshint validthis: true */
        var vm = this;

        vm.cancel = cancel;
        vm.importTemplates = importTemplates;
        //vm.quickStartTemplates = JSON.parse(initialData).items;
        vm.quickStartTemplates = initialData.items;
        //Format the returned JSON results to have a directory name
        vm.quickStartTemplates.forEach(function (quickStartTemplateSearchResult) {
            var path = quickStartTemplateSearchResult.path;
            quickStartTemplateSearchResult.directoryName = path.replace('/azuredeploy.json', '');
        });

        vm.gridOptions = {
            data: 'vm.quickStartTemplates',
            enableFiltering: true,
            columnDefs: [
                { name: 'Quick Start Template Name', field: 'directoryName', width: '65%', sort: { direction: uiGridConstants.ASC } },
                {
                    name: 'Github',
                    cellTemplate:
                        '<div class="ui-grid-cell-contents">' +
                            '<a target="_blank" href="{{row.entity.html_url}}">View on Github</a>' +
                        '</div>',
                    width: '20%'
                },
                {
                    name: 'Visualizer',
                    cellTemplate:
                        '<div class="ui-grid-cell-contents">' +
                            '<a target="_blank" ' +
                            'href="http://armviz.io/#/?load=https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/' +
                            '{{row.entity.path}}">Visualizer</a>' +
                        '</div>',
                    width: '15%'
                }
            ],
            multiSelect: true,
            enableRowSelection: true,
            enableFullRowSelection: true,
            enableSelectAll: true,
            enableClearAll: true,
            showGridFooter: true,
            enableColumnResizing: false, //For now, there is only one column.
            enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
            enableRowHeaderSelection: true,
            selectionRowHeaderWidth: 35,
            rowHeight: 35,

            onRegisterApi: function (gridApi) {
                vm.gridApi = gridApi;
            }
        };

        function cancel() {
            $uibModalInstance.dismiss();
        }

        function importTemplates() {
            var selectedTemplates = vm.gridApi.selection.getSelectedRows();
            $uibModalInstance.close(selectedTemplates);
        }
    }
})();