export const idlFactory = ({ IDL }) => {
  const Role = IDL.Variant({
    'gm' : IDL.Null,
    'player' : IDL.Null,
    'both' : IDL.Null,
  });
  const UserRole = IDL.Variant({
    'admin' : IDL.Null,
    'user' : IDL.Null,
    'guest' : IDL.Null,
  });
  const Time = IDL.Int;
  const RSVP = IDL.Record({
    'name' : IDL.Text,
    'inviteCode' : IDL.Text,
    'timestamp' : Time,
    'attending' : IDL.Bool,
  });
  const UserProfile = IDL.Record({ 'name' : IDL.Text });
  const Campaign = IDL.Record({
    'id' : IDL.Text,
    'owner' : IDL.Principal,
    'name' : IDL.Text,
    'description' : IDL.Text,
    'inGameDateTime' : IDL.Text,
  });
  const InviteCode = IDL.Record({
    'created' : Time,
    'code' : IDL.Text,
    'used' : IDL.Bool,
  });
  const MetadataNote = IDL.Record({
    'id' : IDL.Text,
    'content' : IDL.Text,
    'owner' : IDL.Principal,
    'name' : IDL.Text,
    'campaignId' : IDL.Text,
    'linkedSessionNotes' : IDL.Vec(IDL.Text),
  });
  const LinkedNoteDetails = IDL.Record({
    'hasReadAccess' : IDL.Bool,
    'sessionName' : IDL.Text,
    'noteId' : IDL.Text,
    'tags' : IDL.Vec(IDL.Text),
    'noteName' : IDL.Text,
    'linkedNoteId' : IDL.Text,
    'isOwner' : IDL.Bool,
  });
  const NoteAccess = IDL.Record({
    'permissions' : IDL.Nat8,
    'noteId' : IDL.Text,
    'participant' : IDL.Principal,
  });
  const Note = IDL.Record({
    'id' : IDL.Text,
    'creator' : IDL.Principal,
    'gmContent' : IDL.Text,
    'name' : IDL.Text,
    'tags' : IDL.Vec(IDL.Text),
    'playerContent' : IDL.Text,
    'inGameDateTime' : IDL.Opt(IDL.Text),
    'sessionId' : IDL.Text,
  });
  const Participant = IDL.Record({
    'principal' : IDL.Principal,
    'role' : Role,
  });
  const InvitationStatus = IDL.Variant({
    'pending' : IDL.Null,
    'rejected' : IDL.Null,
    'accepted' : IDL.Null,
  });
  const Invitation = IDL.Record({
    'id' : IDL.Text,
    'status' : InvitationStatus,
    'role' : Role,
    'recipient' : IDL.Principal,
    'campaignId' : IDL.Text,
    'sender' : IDL.Principal,
    'timestamp' : Time,
  });
  const SessionPlayer = IDL.Record({
    'participant' : IDL.Principal,
    'sessionId' : IDL.Text,
  });
  const Session = IDL.Record({
    'id' : IDL.Text,
    'creator' : IDL.Principal,
    'date' : IDL.Text,
    'name' : IDL.Text,
    'campaignId' : IDL.Text,
    'description' : IDL.Text,
  });
  return IDL.Service({
    'acceptInvitation' : IDL.Func([IDL.Text], [], []),
    'addParticipant' : IDL.Func([IDL.Text, IDL.Principal, Role], [], []),
    'addSessionPlayer' : IDL.Func([IDL.Text, IDL.Principal], [], []),
    'addTag' : IDL.Func([IDL.Text, IDL.Text], [], []),
    'assignCallerUserRole' : IDL.Func([IDL.Principal, UserRole], [], []),
    'createCampaign' : IDL.Func(
        [IDL.Text, IDL.Text, IDL.Text, IDL.Text],
        [],
        [],
      ),
    'createMetadataNote' : IDL.Func(
        [IDL.Text, IDL.Text, IDL.Text, IDL.Vec(IDL.Text), IDL.Text],
        [],
        [],
      ),
    'createNote' : IDL.Func(
        [
          IDL.Text,
          IDL.Text,
          IDL.Text,
          IDL.Text,
          IDL.Text,
          IDL.Opt(IDL.Text),
          IDL.Vec(IDL.Text),
        ],
        [],
        [],
      ),
    'createSession' : IDL.Func(
        [IDL.Text, IDL.Text, IDL.Text, IDL.Text, IDL.Text],
        [],
        [],
      ),
    'deleteCampaign' : IDL.Func([IDL.Text], [], []),
    'deleteNote' : IDL.Func([IDL.Text], [], []),
    'deleteSession' : IDL.Func([IDL.Text], [], []),
    'generateInviteCode' : IDL.Func([], [IDL.Text], []),
    'getAllRSVPs' : IDL.Func([], [IDL.Vec(RSVP)], ['query']),
    'getCallerUserProfile' : IDL.Func([], [IDL.Opt(UserProfile)], ['query']),
    'getCallerUserRole' : IDL.Func([], [UserRole], ['query']),
    'getCampaignDetails' : IDL.Func([IDL.Text], [IDL.Opt(Campaign)], ['query']),
    'getInviteCodes' : IDL.Func([], [IDL.Vec(InviteCode)], ['query']),
    'getLinkedMetadataNotes' : IDL.Func(
        [IDL.Text],
        [IDL.Vec(MetadataNote)],
        ['query'],
      ),
    'getLinkedNotes' : IDL.Func(
        [IDL.Text],
        [IDL.Vec(LinkedNoteDetails)],
        ['query'],
      ),
    'getMetadataNoteDetails' : IDL.Func(
        [IDL.Text],
        [IDL.Opt(MetadataNote)],
        ['query'],
      ),
    'getMetadataNotes' : IDL.Func([], [IDL.Vec(MetadataNote)], ['query']),
    'getNoteAccess' : IDL.Func([IDL.Text], [IDL.Vec(NoteAccess)], ['query']),
    'getNotes' : IDL.Func([IDL.Text], [IDL.Vec(Note)], ['query']),
    'getNotesByTag' : IDL.Func([IDL.Text], [IDL.Vec(Note)], ['query']),
    'getOwnedCampaigns' : IDL.Func([], [IDL.Vec(Campaign)], ['query']),
    'getParticipants' : IDL.Func([IDL.Text], [IDL.Vec(Participant)], ['query']),
    'getParticipatedCampaigns' : IDL.Func([], [IDL.Vec(Campaign)], ['query']),
    'getReceivedInvitations' : IDL.Func([], [IDL.Vec(Invitation)], ['query']),
    'getSentInvitations' : IDL.Func([], [IDL.Vec(Invitation)], ['query']),
    'getSessionPlayers' : IDL.Func(
        [IDL.Text],
        [IDL.Vec(SessionPlayer)],
        ['query'],
      ),
    'getSessions' : IDL.Func([IDL.Text], [IDL.Vec(Session)], ['query']),
    'getUserCampaignTags' : IDL.Func([], [IDL.Vec(IDL.Text)], ['query']),
    'getUserProfile' : IDL.Func(
        [IDL.Principal],
        [IDL.Opt(UserProfile)],
        ['query'],
      ),
    'initializeAccessControl' : IDL.Func([], [], []),
    'isCallerAdmin' : IDL.Func([], [IDL.Bool], ['query']),
    'isUsernameAvailable' : IDL.Func([IDL.Text], [IDL.Bool], ['query']),
    'leaveCampaign' : IDL.Func([IDL.Text], [], []),
    'linkMetadataNoteByName' : IDL.Func([IDL.Text, IDL.Text, IDL.Text], [], []),
    'linkNotes' : IDL.Func([IDL.Text, IDL.Text], [], []),
    'manageNoteAccess' : IDL.Func([IDL.Text, IDL.Principal, IDL.Nat8], [], []),
    'rejectInvitation' : IDL.Func([IDL.Text], [], []),
    'removeNoteLink' : IDL.Func([IDL.Text, IDL.Text], [], []),
    'removeParticipant' : IDL.Func([IDL.Text, IDL.Principal], [], []),
    'removeSessionPlayer' : IDL.Func([IDL.Text, IDL.Principal], [], []),
    'removeTag' : IDL.Func([IDL.Text, IDL.Text], [], []),
    'saveCallerUserProfile' : IDL.Func([UserProfile], [], []),
    'sendInvitation' : IDL.Func([IDL.Text, IDL.Text, IDL.Text, Role], [], []),
    'submitRSVP' : IDL.Func([IDL.Text, IDL.Bool, IDL.Text], [], []),
    'unlinkMetadataNote' : IDL.Func([IDL.Text, IDL.Text], [], []),
    'updateCampaign' : IDL.Func(
        [IDL.Text, IDL.Text, IDL.Text, IDL.Text],
        [],
        [],
      ),
    'updateNote' : IDL.Func(
        [
          IDL.Text,
          IDL.Text,
          IDL.Text,
          IDL.Text,
          IDL.Opt(IDL.Text),
          IDL.Vec(IDL.Text),
        ],
        [],
        [],
      ),
    'updateParticipantRole' : IDL.Func([IDL.Text, IDL.Principal, Role], [], []),
    'updateSession' : IDL.Func(
        [IDL.Text, IDL.Text, IDL.Text, IDL.Text],
        [],
        [],
      ),
  });
};
export const init = ({ IDL }) => { return []; };
