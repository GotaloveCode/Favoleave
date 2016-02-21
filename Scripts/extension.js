$(function () {
    $('.validation-summary-errors').each(function () {
        $(this).addClass('alert');
        $(this).addClass('alert-danger');
    });

    $('.input-validation-error').parents('.form-group').addClass('has-error');
    $('.field-validation-error').addClass('text-danger');

});