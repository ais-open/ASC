(function () {
    'use strict';

    angular.module('ascApp').controller('EditProductCtrl', EditProductCtrl);

    EditProductCtrl.$inject = ['initialData', 'ascApi', 'dialogsService', '$timeout', '$state', 'toastr'];

    /* @ngInject */
    function EditProductCtrl(initialData, ascApi, dialogs, $timeout, $state, toastr) {
        /* jshint validthis: true */
        var vm = this;

        vm.jsonErrors = [];
        vm.aceConfig = {
            //mode: 'json',
            showGutter: true,
            //theme: 'tomorrow',
            onLoad: function (editor) {
                editor.setOptions({
                    fontSize: 14,
                    maxLines: Infinity
                });
                editor.getSession().on('changeAnnotation', function () {
                    $timeout(function () {
                        vm.jsonErrors = editor.getSession().getAnnotations();
                    });
                });
            }
        };

        vm.deleteTemplate = deleteTemplate;
        vm.dialogs = dialogs;
        vm.isClean = isClean;
        vm.reset = reset;
        vm.save = save;
        vm.downloadTemplate = downloadTemplate;
        vm.selectedItem = initialData || {};
        vm.origData = angular.copy(vm.selectedItem);
        vm.canSave = canSave;
        //vm.showDrop = false;
        vm.showJSON = true;
        //vm.showEmptyJSON = false;
        //vm.editTemplates = false;
        activate();

        ////////////////

        function activate() { }

        function canSave(productForm) {
            return !vm.isClean() &&
                !productForm.productName.$invalid && vm.jsonErrors.length === 0 &&
                vm.selectedItem.templateData.length > 0;
        }

        function deleteTemplate() {
            dialogs.confirm('Are you sure you want to delete this Template?', 'Delete?', ['Yes', 'No']).then(function () {
                ascApi.deleteTemplate(vm.selectedItem.rowKey).then(function (data) {
                    $state.go('home');
                });
            });
        }

        function isClean() {
            return (vm.origData.templateData === vm.selectedItem.templateData &&
                vm.origData.name === vm.selectedItem.name &&
                vm.origData.productPrice === vm.selectedItem.productPrice &&
                vm.selectedItem.description === vm.origData.description &&
                vm.selectedItem.productImage === vm.origData.productImage &&
                vm.selectedItem.isPublished === vm.origData.isPublished);
        }

        function reset() {
            vm.selectedItem = vm.origData;
        }

        function save() {
            ascApi.saveTemplate(vm.selectedItem).then(function (data) {
                if (data) {
                    var isNewProduct = !(vm.selectedItem.rowKey);
                    var saveMessage = 'The ' + (isNewProduct ? 'new' : '') + ' product was saved successfully.';
                    toastr.success(saveMessage, 'Save Successful');
                    if (isNewProduct) {
                        $state.go('edit-product', { id: data.rowKey });
                    } else {
                        vm.selectedItem = data;
                        vm.origData = angular.copy(vm.selectedItem);
                    }
                }
            });
        }

        function downloadTemplate() {
            return ascApi.downloadTemplate(vm.selectedItem.rowKey);
        }
    }
})();