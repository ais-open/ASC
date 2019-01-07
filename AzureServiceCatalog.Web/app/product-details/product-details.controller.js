(function() {
    'use strict';

    angular.module('ascApp').controller('ProductDetailsCtrl', ProductDetailsCtrl);

    ProductDetailsCtrl.$inject = ['$state', 'initialData', 'ascApi', 'productListService', 'dialogsService', 'appStorage'];

    function ProductDetailsCtrl($state, initialData, ascApi, productListService, dialogsService, appStorage) {

        /* jshint validthis: true */
        var vm = this;

        vm.product = initialData;
        vm.resourceExpanders = {};
        vm.displayObjectTemplate = displayObjectTemplate;
        vm.displaySimpleTemplate = displaySimpleTemplate;
        vm.productListService = productListService;
        vm.product.templateData = JSON.parse(vm.product.templateData);
        vm.product.data = angular.copy(vm.product.templateData);
        vm.getParameterValue = getParameterValue;
        vm.getAllowedValueDisplayName = getAllowedValueDisplayName;
        vm.getResourceParameterDisplayValue = getResourceParameterDisplayValue;
        vm.getProductParameterDisplayValue = getProductParameterDisplayValue;
        vm.isProductParameter = isProductParameter;
        vm.lodash = _;
        vm.isArrayParameterValue = isArrayParameterValue;
        vm.addParameterArrayItem = addParameterArrayItem;
        vm.removeParameterArrayItem = removeParameterArrayItem;
        vm.displayArrayTemplate = displayArrayTemplate;
        vm.isNameValueObject = isNameValueObject;
        vm.constructNameValuePair = constructNameValuePair;
        vm.dialogsService = dialogsService;
        vm.toggleExpand = toggleExpand;
        vm.userDetail = appStorage.getUserDetail();
        vm.collapseAllResources = collapseAllResources;
        vm.expandAllResources = expandAllResources;
        activate();

        ////////////////

        function activate() { }

        function toggleExpand(key) {
            vm.resourceExpanders[key] = !vm.resourceExpanders[key];
        }

        function collapseAllResources() {
            for (var key in vm.resourceExpanders) {
                if (vm.resourceExpanders.hasOwnProperty(key)) {
                    vm.resourceExpanders[key] = false;
                }
            }
        }
        function expandAllResources() {
            for (var key in vm.resourceExpanders) {
                if (vm.resourceExpanders.hasOwnProperty(key)) {
                    vm.resourceExpanders[key] = true;
                }
            }
        }

        function addParameterArrayItem(productParameter) {
            var defaultValueFirstModel = productParameter.defaultValue[0];
            var deepCopyOfFirstDefaultValue = angular.copy(defaultValueFirstModel);
            //Null out the values
            for (var key in deepCopyOfFirstDefaultValue) {
                if (deepCopyOfFirstDefaultValue.hasOwnProperty(key)) {
                    deepCopyOfFirstDefaultValue[key] = null;
                }
            }
            productParameter.value.push(deepCopyOfFirstDefaultValue);
        }

        function removeParameterArrayItem(index, array) {
            if (array.length > 1) {
                array.splice(index, 1);
            }
        }

        /// <summary>
        /// Retrieves the corresponding parameter from the product's root parameters reference object.
        /// </summary>
        /// <param name="resourcePropertyParameterString" type="string">
        /// The resource property that in the format "[parameters('parameterKey')]"
        /// </param>
        /// <param name="productParameters" type="Object">
        /// The root parameters reference object that has the parameterKey-Value pair.
        /// </param>
        /// <returns type="Object"></returns>
        function getParameterValue(resourcePropertyParameterString, productParameters) {
            //The product list service has a function that extracts the parameter key string
            return productParameters[productListService.getParameterKey(resourcePropertyParameterString)];
        }

        //#region Product and Parameter type boolean check functions
        function isProductParameter(propertyValue) {
            return _.startsWith(propertyValue, '[parameters');
        }

        function isVariable(propertyValue) {
            return _.startsWith(propertyValue, '[variables(');
        }

        function isArrayParameterValue(parameterValue) {
            return parameterValue && parameterValue.constructor === Array;
        }
        //#endregion

        //#region JSON interpreter functions for HTML label and value displays
        function getResourceParameterDisplayValue(resourcePropertyKey, resourcePropertyValue) {
            var propType = typeof resourcePropertyValue;
            //console.log('***propType', propType, resourcePropertyKey, resourcePropertyValue);

            if (propType === 'object') {
                return '';
            }

            if (isProductParameter(resourcePropertyValue)) {
                //return '<Determined by customization>';
                return '<Customized>';
                //var productParameter = getParameterValue(resourcePropertyValue, productParameters);
                //var productParameter = getParameterValue(resourcePropertyValue, vm.product.data.parameters);
                //return getProductParameterDisplayValue(productParameter);
            }

            if (isVariable(resourcePropertyValue)){
                var variableName = resourcePropertyValue.match(/\[variables\('([^']+)'/)[1];
                var variableValue = vm.product.templateData.variables[variableName];
                if (_.startsWith(variableValue, '[')) {
                    return getResourceParameterDisplayValue(variableName, variableValue);
                } else {
                    return variableValue;
                }
            }

            if (_.startsWith(resourcePropertyValue, '[')) {
                // Not a parameter or a variable - probably '[concat'
                return '<Formula>';
            }

            // If we've made it this far, just return the raw value
            return resourcePropertyValue;
        }

        function constructNameValuePair(object) {
            var nameValueObject = {};
            nameValueObject[object.name] = object.value;
            return nameValueObject;
        }

        function isNameValueObject(object) {
            //Some objects have the following format: { name: nameOfProperty, value: valueOfProperty }
            return (object.name && object.value) && (displaySimpleTemplate(object.name) && displaySimpleTemplate(object.value));
        }

        function displaySimpleTemplate(value) {
            var t = typeof value;
            return t === 'string' || t === 'boolean' || t === 'number';
        }

        function displayObjectTemplate(key, value) {
            return value && value.constructor !== Array && typeof value === 'object';
        }
        function displayArrayTemplate(key, value) {
            return value && value.constructor === Array;
        }
        function getProductParameterDisplayValue(productParameter) {
            //Note, make sure to use null checks and not "falsy" checks as certain parameters
            //can have valid values of 0, which will be interpreted as false due to JavaScript's boolean type coercion scheme.
            if (productParameter) {
                var metadata = productParameter.metadata;
                if (metadata) {
                    if (metadata.displayValue) {
                        return metadata.displayValue;
                    }
                    var allowedValues = productParameter.allowedValues;
                    var allowedValuesFriendlyDisplayNames = metadata.allowedValuesFriendlyDisplayNames;
                    //var parameterValue = productParameter.value;
                    var defaultValue = productParameter.defaultValue;
                    return getAllowedValueFriendlyDisplayNameOrDefault(defaultValue, allowedValues, allowedValuesFriendlyDisplayNames);
                }
            }
        }

        function getAllowedValueFriendlyDisplayNameOrDefault(defaultValue, allowedValues, allowedValuesFriendlyDisplayNames) {

            if (allowedValues &&
                allowedValuesFriendlyDisplayNames &&
                allowedValuesFriendlyDisplayNames.length === allowedValues.length) {
                if (defaultValue) {
                    for (var i = 0; i < allowedValues.length; i++) {
                        if (defaultValue === allowedValues[i]) {
                            return allowedValuesFriendlyDisplayNames[i];
                        }
                    }
                }
            }
            if (defaultValue) {
                return defaultValue;
            }
        }

        //Maps the allowed values array in the product JSON to a friendly display name for the user to select.
        function getAllowedValueDisplayName(allowedValueParentProperty, index) {
            if (allowedValueParentProperty &&
                allowedValueParentProperty.metadata &&
                allowedValueParentProperty.metadata.allowedValuesFriendlyDisplayNames) {
                return allowedValueParentProperty.metadata.allowedValuesFriendlyDisplayNames[index];
            }
            return allowedValueParentProperty.allowedValues[index] || index;
        }
        //#endregion
    }
})();