(function () {
    'use strict';

    angular.module('ascApp').controller('ProductListCtrl', ProductListCtrl);

    ProductListCtrl.$inject = ['initialData', 'ascApi', 'dialogsService', 'appSpinner', '$timeout', '$uibModal', '$q', '$state', 'toastr'];

    /* @ngInject */
    function ProductListCtrl(initialData, ascApi, dialogs, appSpinner, $timeout, $uibModal, $q, $state, toastr) {
        /* jshint validthis: true */
        var vm = this;

        vm.templates = initialData;
        vm.editTemplates = false;
        vm.importJSON = importJSON;
        vm.importFile = importFile;
        vm.importQuickStartTemplates = importQuickStartTemplates;
        activate();

        ////////////////

        function activate() { }

        function importFile(event) {
            var parent = angular.element(event.target.parentElement);
            parent.find('input')[0].click();
        }

        function importJSON(rawJSONPayload, event) {
            var fileName = event.target.files[0].name.replace('.json', '');
            var newTemplate = {
                name: fileName,
                templateData: rawJSONPayload
            };
            return ascApi.saveTemplate(newTemplate).then(function() {
                $state.reload();
                toastr.success('Successfully imported file: ' + newTemplate.name + '.', 'Save Successful');
            });
        }

        function importQuickStartTemplates() {
            //Show the modal
            var importDialog = $uibModal.open({
                templateUrl: '/app/manage-products/quick-starts-modal.html',
                controller: 'QuickStartsModalCtrl',
                controllerAs: 'vm',
                resolve: {
                    initialData: function() {
                        return ascApi.getQuickStartTemplates();
                    }
                },
                size: 'lg'
            });
            return importDialog.result.then(function (quickStartTemplateSelections) {
                //#region Old Save Sequence
                var quickStartTemplateGetPromises = [];
                var templatesRetrieved = 0;
                function updateRetrievealMessage() {
                    appSpinner.showSpinner('Retrieving quick start template ' + templatesRetrieved + ' of ' +
                        quickStartTemplateSelections.length + '...');
                }
                quickStartTemplateSelections.forEach(function (selection) {
                    //Construct the download url
                    var rawFileUrl = 'https://raw.githubusercontent.com/' +
                        selection.repository.owner.login + '/' + selection.repository.name + '/master/' + selection.path;
                    quickStartTemplateGetPromises.push(ascApi.getQuickStartTemplate(rawFileUrl).then(function (payload) {
                        templatesRetrieved++;
                        updateRetrievealMessage();
                        return {
                            directoryName: selection.directoryName,
                            JSONPayload: payload
                        };
                    }));
                });
                $q.all(quickStartTemplateGetPromises).then(function (templates) {
                    var totalPayloads = templates.length;
                    var payloadsSaved = 0;
                    function updateSaveMessage() {
                        var saveMessage = 'Saving quick start templates. Save complete on ' + payloadsSaved + ' of ' + totalPayloads;
                        appSpinner.showSpinner(saveMessage);
                    }

                    updateSaveMessage();
                    var payloadSavePromises = [];
                    templates.forEach(function (template) {
                        var newTemplate = {
                            name: template.directoryName,
                            templateData: JSON.stringify(template.JSONPayload, null, '\t')
                        };

                        console.log('about to save new quick start template', newTemplate);
                        payloadSavePromises.push(ascApi.saveTemplate(newTemplate).then(function() {
                            payloadsSaved++;
                            updateSaveMessage();
                        }));
                    });
                    $q.all(payloadSavePromises).then(function() {
                        appSpinner.hideSpinner();
                        $state.reload();
                    });
                });
                //#endregion
            });
        }
    }
})();