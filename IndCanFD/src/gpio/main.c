#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/select.h>
#include <termios.h>
#include <linux/can.h>
#include <linux/can/raw.h>
#include "can_operations.h"
#include "database_operations.h"
#include "gpio_operations.h"

int main(void) {
    // ... Open the CAN sockets ...

    struct can_frame frame;

    // Set the terminal to raw mode to allow reading single characters
    struct termios old_tio, new_tio;
    tcgetattr(STDIN_FILENO, &old_tio);
    new_tio = old_tio;
    new_tio.c_lflag &= ~(ICANON | ECHO);
    tcsetattr(STDIN_FILENO, TCSANOW, &new_tio);

    while (1) {
        fd_set readset;
        FD_ZERO(&readset);
        FD_SET(STDIN_FILENO, &readset);  // Add the standard input to the set
        FD_SET(s, &readset);   // Add the can0 socket to the set
        FD_SET(s1, &readset);  // Add the can1 socket to the set
        // TODO: Add the GPIO file descriptor to the set

        // Wait for activity on any of the file descriptors
        if (select(FD_SETSIZE, &readset, NULL, NULL, NULL) < 0) {
            perror("Error in select");
            break;
        }

        // Check if there was activity on the standard input
        if (FD_ISSET(STDIN_FILENO, &readset)) {
            char c;
            if (read(STDIN_FILENO, &c, 1) > 0 && c == 'q') {
                fprintf(stderr, "Exiting...\n");
                break;
            }
        }

        // ... Handle activity on the other file descriptors ...
    }

    // Restore the old terminal settings
    tcsetattr(STDIN_FILENO, TCSANOW, &old_tio);

    // ... Close the sockets ...

    return 0;
}
