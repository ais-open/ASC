(function () {
    'use strict';

    angular.module('ascApp').directive('spinner', spinner);

    spinner.$inject = ['$window'];
    // Adding a comment to the custom angular directive "spinner" with a text editor, in this case Visual Studio Code.
    // Git will subsequently recognize, through watching the filesystem, that a change on this file has taken place,
    // so that I can push this change to my new branch: Adam-TestBranch
    function spinner($window) {
        var directive = {
            link: link,
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            scope.spinner = null;
            scope.$watch(attrs.spinner, function (options) {
                if (scope.spinner) {
                    scope.spinner.stop();
                }
                scope.spinner = new $window.Spinner(options);
                scope.spinner.spin(element[0]);
            }, true);
        }
    }
})();