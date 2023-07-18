#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sqlite3.h>
#include "database_operations.h"


char* read_data_from_db(int id) {
    sqlite3* db;
    char* result = NULL;
    if (sqlite3_open("PcCan.db", &db) != SQLITE_OK) {
        fprintf(stderr, "Cannot open database: %s\n", sqlite3_errmsg(db));
        return NULL;
    }
    sqlite3_stmt* stmt;
    const char* sql = "SELECT ID, Data FROM Config WHERE ID = ?";
    if (sqlite3_prepare_v2(db, sql, -1, &stmt, NULL) != SQLITE_OK) {
        fprintf(stderr, "Failed to prepare statement: %s\n", sqlite3_errmsg(db));
        sqlite3_close(db);
        return NULL;
    }
    if (sqlite3_bind_int(stmt, 1, id) != SQLITE_OK) {
        fprintf(stderr, "Failed to bind parameter: %s\n", sqlite3_errmsg(db));
        sqlite3_close(db);
        return NULL;
    }
    while (sqlite3_step(stmt) == SQLITE_ROW) {
        result = strdup((char*)sqlite3_column_text(stmt, 1));
        break;
    }
    sqlite3_finalize(stmt);
    sqlite3_close(db);
    return result;
}


char* read_port_from_db(int id) {
    sqlite3* db;
    char* result = NULL;
    if (sqlite3_open("PcCan.db", &db) != SQLITE_OK) {
        fprintf(stderr, "Cannot open database: %s\n", sqlite3_errmsg(db));
        return NULL;
    }
    sqlite3_stmt* stmt;
    const char* sql = "SELECT Data FROM Ports WHERE ID = ?";
    if (sqlite3_prepare_v2(db, sql, -1, &stmt, NULL) != SQLITE_OK) {
        fprintf(stderr, "Failed to prepare statement: %s\n", sqlite3_errmsg(db));
        sqlite3_close(db);
        return NULL;
    }
    if (sqlite3_bind_int(stmt, 1, id) != SQLITE_OK) {
        fprintf(stderr, "Failed to bind parameter: %s\n", sqlite3_errmsg(db));
        sqlite3_close(db);
        return NULL;
    }
    while (sqlite3_step(stmt) == SQLITE_ROW) {
        result = strdup((char*)sqlite3_column_text(stmt, 0));
        break;
    }
    sqlite3_finalize(stmt);
    sqlite3_close(db);
    return result;
}