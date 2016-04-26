
//collapsable menu
$(function () {
    $('.collapse-menu').on('click', function () {
        //console.log($('#menu').width());
        if ($('#menu').width() == '240') {
            $('#menu').animate({'width': '63px' }, 'slow', function () {
                $('#menu').addClass('mini-menu');
            });
            $('#main').animate({ 'margin-left': '20px' }, 'slow');
        }
        else {
            $('#menu').animate({ 'width': '240px' }, 'slow');
            $('#main').animate({ 'margin-left': '260px' }, 'slow');
        }
    });
});

