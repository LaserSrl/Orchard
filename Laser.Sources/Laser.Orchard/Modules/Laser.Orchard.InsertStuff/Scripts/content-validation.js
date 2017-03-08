$(document).ready(function () {


	$('form').prepend('<div id="clientValSummary" class="validation-summary-errors"><ul></ul></div>');
	$('#clientValSummary').hide();
	$('form').validate({
		invalidHandler: function() {
			window.scrollTo(0, 0);
		},
		rules: {
			'OwnerEditor.Owner': "required"
		},
		messages: {
			'OwnerEditor.Owner': "Owner is required."
		},
		errorContainer: "#clientValSummary",
		errorLabelContainer: "#clientValSummary ul",
		wrapper: "li",
		errorClass: 'input-validation-error',
		focusInvalid: false,
		highlight: function (element, errorClass, validClass) {
			$(element).addClass('input-validation-error');
		},
		unhighlight: function (element, errorClass, validClass) {
			$(element).removeClass('input-validation-error');
		}
		

	});


});
