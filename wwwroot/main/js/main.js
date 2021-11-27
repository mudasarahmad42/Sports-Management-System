$(window).scroll(function () {
  var scroll = $(window).scrollTop();
  if (scroll < 50) {
    $('.sticky-top').css('background', 'transparent');
  } else {
    $('.sticky-top').css('background', '#fff');
  }
});

if ($(window).width() > 768) {
  /*Slick Carousel*/
  $(document).ready(function () {
    $('.slider-js').slick({
      autoplay: true,
      autoplaySpeed: 1500,
      infinite: true,
      arrows: false,
      fade: true,
      cssEase: 'linear',
    });
  });
}
else {
  $(document).ready(function () {
    $('.slider-js').slick("unslick");
  });
}

 

$(document).ready(function () {
  $('.slick-cards').slick({
    slidesToShow: 3,
    slidesToScroll: 1,
    autoplay: true,
    autoplaySpeed: 1000,
  });
});

/*Back to the top button */

var btn = $('#backtotopbutton');

$(window).scroll(function() {
  if ($(window).scrollTop() > 300) {
    btn.addClass('show');
  } else {
    btn.removeClass('show');
  }
});

btn.on('click', function(e) {
  e.preventDefault();
  $('html, body').animate({scrollTop:0}, '300');
});


/*Accordion*/
$("#accordion").on("hide.bs.collapse show.bs.collapse", e => {
  $(e.target)
    .prev()
    .find("i:last-child")
    .toggleClass("fa-chevron-down fa-chevron-up");
});




