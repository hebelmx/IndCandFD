#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <linux/can.h>
#include "can_operations.h"

int send_can_frame(int s, struct can_frame* frame) {
    if (write(s, frame, sizeof(struct can_frame)) != sizeof(struct can_frame)) {
        perror("Error while writing message");
        return -1;
    }
    return 0;
}

int read_and_compare_can_frame(int s, struct can_frame* frame) {
    struct can_frame recv_frame;
    if (read(s, &recv_frame, sizeof(struct can_frame)) != sizeof(struct can_frame)) {
        perror("Error while reading message");
        return -1;
    }
    if (recv_frame.can_id != frame->can_id || recv_frame.can_dlc != frame->can_dlc ||
        memcmp(recv_frame.data, frame->data, frame->can_dlc) != 0) {
        fprintf(stderr, "Error: sent and received frames don't match\n");
        return -1;
    }
    return 0;
}
