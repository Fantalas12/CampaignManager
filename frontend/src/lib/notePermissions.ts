// Note permission constants using bitflags
export const NOTE_PERMISSIONS = {
  PLAYER_READ: 1n,    // 0001
  GM_READ: 2n,        // 0010
  WRITE: 4n,          // 0100
  EXECUTE: 8n,        // 1000
} as const;

export type NotePermission = typeof NOTE_PERMISSIONS[keyof typeof NOTE_PERMISSIONS];

export const hasPermission = (permissions: bigint, permission: bigint): boolean => {
  return (permissions & permission) !== 0n;
};

export const addPermission = (permissions: bigint, permission: bigint): bigint => {
  return permissions | permission;
};

export const removePermission = (permissions: bigint, permission: bigint): bigint => {
  return permissions & ~permission;
};

export const togglePermission = (permissions: bigint, permission: bigint): bigint => {
  return permissions ^ permission;
};

export const getPermissionLabel = (permission: bigint): string => {
  switch (permission) {
    case NOTE_PERMISSIONS.PLAYER_READ:
      return 'Player Read';
    case NOTE_PERMISSIONS.GM_READ:
      return 'GM Read';
    case NOTE_PERMISSIONS.WRITE:
      return 'Write';
    case NOTE_PERMISSIONS.EXECUTE:
      return 'Execute';
    default:
      return 'Unknown';
  }
};

export const getPermissionDescription = (permission: bigint): string => {
  switch (permission) {
    case NOTE_PERMISSIONS.PLAYER_READ:
      return 'Can view note content as a player';
    case NOTE_PERMISSIONS.GM_READ:
      return 'Can view note content as a game master';
    case NOTE_PERMISSIONS.WRITE:
      return 'Can edit note content';
    case NOTE_PERMISSIONS.EXECUTE:
      return 'Can perform special actions with this note';
    default:
      return '';
  }
};
