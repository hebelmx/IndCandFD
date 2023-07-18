#include "can_frame_utils.h"
#include <stdlib.h>
#include <string.h>
#include <stdio.h>

void fill_can_frame(struct can_frame *frame, char *data) {
    frame->can_id = 0x001;
    frame->can_dlc = 8;

    // Skip leading spaces
    while (*data == ' ') {
        data++;
    }

    // Read and convert the data bytes
    for (int i = 0; i < frame->can_dlc && *data; i++) {
        unsigned int value;
        if (sscanf(data, "%02x", &value) == 1) {
            frame->data[i] = value;
        }
        data += 3;  // Move to the start of the next byte
    }
}
