#! /usr/bin/env gforth
\ Sample gtk application using gtk-server
\ Written by Jorge Acereda ( jacer...@users.sourceforge.net )

require unix/socket.fs
vocabulary gui  also gui  definitions

sh gtk-server tcp localhost:5000 &
sh sleep 1

0 value handle
create buf 256 allot

: gtk{ ( -- , send subsequent output to gtk-server)
    handle to outfile-id ;

: }gtk ( -- , finish output to gtk-server, receive response)
    cr stdout to outfile-id
    buf 1+ 255 handle read-line abort" GTK read error" drop
    buf c! ;

\ without waiting for an answer
: }gtk0 ( -- , finish output to gtk-server, receive response)
    cr stdout to outfile-id ;

: gtkres ( -- x, evaluate gtk-server response )
    buf count evaluate ;

: s ( "delimiter" "string" -- , compile string with strange delimiter)
    char parse postpone sliteral ; immediate compile-only

: init ( -- , init connection with gtk-server)
    s" localhost" 5000 open-socket to handle
    gtk{ ." gtk_init NULL NULL" }gtk ;

: show ( widget -- )
    gtk{ ." gtk_widget_show " . }gtk ;

: window" ( "title" -- win )
    gtk{ ." gtk_window_new 0" }gtk gtkres
    gtk{ ." gtk_window_set_title " dup . [char] " parse type }gtk ;

: table ( homogeneous colums rows -- widget )
    gtk{ ." gtk_table_new " . . . }gtk gtkres ;

: add ( widget container -- )
    gtk{ ." gtk_container_add " .  . }gtk ;

: label" ( "label" -- widget )
    gtk{ s ' gtk_label_new "' type
         [char] " parse type [char] " emit }gtk gtkres ;

: attach ( bot top right left widget table -- )
    gtk{ ." gtk_table_attach_defaults " . . . . . . }gtk ;

: button" ( "name" -- widget )
    gtk{ s ' gtk_button_new_with_label "' type
          [char] " parse type [char] " emit }gtk gtkres ;

: iteration ( -- , perform mainloop iteration )
    gtk{ ." gtk_main_iteration" }gtk ;

: check ( -- widget , check for activity )
    gtk{ ." gtk_server_callback WAIT" }gtk gtkres ;

\ Exit GTK without waiting for an answer
: gtkexit ( -- , disconnect from gtk-server )
    gtk{ ." gtk_exit 0" cr }gtk0 ;

init
window" title" constant win
1 30 30 table constant tab
tab win add
label" Hello world" constant lab
7 3 29 1 lab tab attach
button" exit" constant but
button" nothing" constant but0
27 23 8  2  but0 tab attach
27 23 28 20 but  tab attach
lab show  but0 show  but show  tab show  win show
: run  begin  iteration check but =  until  gtkexit  bye ;
run

