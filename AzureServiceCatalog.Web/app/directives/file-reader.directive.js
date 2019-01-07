(function () {
    'use strict';

    angular.module('ascApp').directive('fileReader', fileReader);

    fileReader.$inject = [];
    // Adding a comment to the custom angular directive "spinner" with a text editor, in this case Visual Studio Code.
    // Git will subsequently recognize, through watching the filesystem, that a change on this file has taken place,
    // so that I can push this change to my new branch: Adam-TestBranch
    function fileReader() {
        var directive = {
            link: link,
            scope: {
                fileReader: '='
            }
        };
        return directive;

        function link(scope, element, attributes) {
            element.bind('change', function (changeEvent) {
                var reader = new FileReader();
                reader.onload = function (loadEvent) {
                    scope.$apply(function () {
                        //Change event passed in as well in case the calling vm needs to perform work on the input element
                        scope.fileReader(loadEvent.target.result, changeEvent);
                    });
                };
                var files = changeEvent.target.files;
                if (files.length === 1) {
                    var file = files[0];
                    if (file.type && file.type.split('/')[0] === 'image') {
                        reader.readAsDataURL(file);
                    } else {
                        reader.readAsText(file);
                    }
                }
            });
        }
    }
})();