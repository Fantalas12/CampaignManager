import type { Principal } from '@dfinity/principal';
import type { ActorMethod } from '@dfinity/agent';
import type { IDL } from '@dfinity/candid';

export interface Campaign {
  'id' : string,
  'owner' : Principal,
  'name' : string,
  'description' : string,
  'inGameDateTime' : string,
}
export interface Invitation {
  'id' : string,
  'status' : InvitationStatus,
  'role' : Role,
  'recipient' : Principal,
  'campaignId' : string,
  'sender' : Principal,
  'timestamp' : Time,
}
export type InvitationStatus = { 'pending' : null } |
  { 'rejected' : null } |
  { 'accepted' : null };
export interface InviteCode {
  'created' : Time,
  'code' : string,
  'used' : boolean,
}
export interface LinkedNoteDetails {
  'hasReadAccess' : boolean,
  'sessionName' : string,
  'noteId' : string,
  'tags' : Array<string>,
  'noteName' : string,
  'linkedNoteId' : string,
  'isOwner' : boolean,
}
export interface MetadataNote {
  'id' : string,
  'content' : string,
  'owner' : Principal,
  'name' : string,
  'campaignId' : string,
  'linkedSessionNotes' : Array<string>,
}
export interface Note {
  'id' : string,
  'creator' : Principal,
  'gmContent' : string,
  'name' : string,
  'tags' : Array<string>,
  'playerContent' : string,
  'inGameDateTime' : [] | [string],
  'sessionId' : string,
}
export interface NoteAccess {
  'permissions' : number,
  'noteId' : string,
  'participant' : Principal,
}
export interface Participant { 'principal' : Principal, 'role' : Role }
export interface RSVP {
  'name' : string,
  'inviteCode' : string,
  'timestamp' : Time,
  'attending' : boolean,
}
export type Role = { 'gm' : null } |
  { 'player' : null } |
  { 'both' : null };
export interface Session {
  'id' : string,
  'creator' : Principal,
  'date' : string,
  'name' : string,
  'campaignId' : string,
  'description' : string,
}
export interface SessionPlayer {
  'participant' : Principal,
  'sessionId' : string,
}
export type Time = bigint;
export interface UserProfile { 'name' : string }
export type UserRole = { 'admin' : null } |
  { 'user' : null } |
  { 'guest' : null };
export interface _SERVICE {
  'acceptInvitation' : ActorMethod<[string], undefined>,
  'addParticipant' : ActorMethod<[string, Principal, Role], undefined>,
  'addSessionPlayer' : ActorMethod<[string, Principal], undefined>,
  'addTag' : ActorMethod<[string, string], undefined>,
  'assignCallerUserRole' : ActorMethod<[Principal, UserRole], undefined>,
  'createCampaign' : ActorMethod<[string, string, string, string], undefined>,
  'createMetadataNote' : ActorMethod<
    [string, string, string, Array<string>, string],
    undefined
  >,
  'createNote' : ActorMethod<
    [string, string, string, string, string, [] | [string], Array<string>],
    undefined
  >,
  'createSession' : ActorMethod<
    [string, string, string, string, string],
    undefined
  >,
  'deleteCampaign' : ActorMethod<[string], undefined>,
  'deleteNote' : ActorMethod<[string], undefined>,
  'deleteSession' : ActorMethod<[string], undefined>,
  'generateInviteCode' : ActorMethod<[], string>,
  'getAllRSVPs' : ActorMethod<[], Array<RSVP>>,
  'getCallerUserProfile' : ActorMethod<[], [] | [UserProfile]>,
  'getCallerUserRole' : ActorMethod<[], UserRole>,
  'getCampaignDetails' : ActorMethod<[string], [] | [Campaign]>,
  'getInviteCodes' : ActorMethod<[], Array<InviteCode>>,
  'getLinkedMetadataNotes' : ActorMethod<[string], Array<MetadataNote>>,
  'getLinkedNotes' : ActorMethod<[string], Array<LinkedNoteDetails>>,
  'getMetadataNoteDetails' : ActorMethod<[string], [] | [MetadataNote]>,
  'getMetadataNotes' : ActorMethod<[], Array<MetadataNote>>,
  'getNoteAccess' : ActorMethod<[string], Array<NoteAccess>>,
  'getNotes' : ActorMethod<[string], Array<Note>>,
  'getNotesByTag' : ActorMethod<[string], Array<Note>>,
  'getOwnedCampaigns' : ActorMethod<[], Array<Campaign>>,
  'getParticipants' : ActorMethod<[string], Array<Participant>>,
  'getParticipatedCampaigns' : ActorMethod<[], Array<Campaign>>,
  'getReceivedInvitations' : ActorMethod<[], Array<Invitation>>,
  'getSentInvitations' : ActorMethod<[], Array<Invitation>>,
  'getSessionPlayers' : ActorMethod<[string], Array<SessionPlayer>>,
  'getSessions' : ActorMethod<[string], Array<Session>>,
  'getUserCampaignTags' : ActorMethod<[], Array<string>>,
  'getUserProfile' : ActorMethod<[Principal], [] | [UserProfile]>,
  'initializeAccessControl' : ActorMethod<[], undefined>,
  'isCallerAdmin' : ActorMethod<[], boolean>,
  'isUsernameAvailable' : ActorMethod<[string], boolean>,
  'leaveCampaign' : ActorMethod<[string], undefined>,
  'linkMetadataNoteByName' : ActorMethod<[string, string, string], undefined>,
  'linkNotes' : ActorMethod<[string, string], undefined>,
  'manageNoteAccess' : ActorMethod<[string, Principal, number], undefined>,
  'rejectInvitation' : ActorMethod<[string], undefined>,
  'removeNoteLink' : ActorMethod<[string, string], undefined>,
  'removeParticipant' : ActorMethod<[string, Principal], undefined>,
  'removeSessionPlayer' : ActorMethod<[string, Principal], undefined>,
  'removeTag' : ActorMethod<[string, string], undefined>,
  'saveCallerUserProfile' : ActorMethod<[UserProfile], undefined>,
  'sendInvitation' : ActorMethod<[string, string, string, Role], undefined>,
  'submitRSVP' : ActorMethod<[string, boolean, string], undefined>,
  'unlinkMetadataNote' : ActorMethod<[string, string], undefined>,
  'updateCampaign' : ActorMethod<[string, string, string, string], undefined>,
  'updateNote' : ActorMethod<
    [string, string, string, string, [] | [string], Array<string>],
    undefined
  >,
  'updateParticipantRole' : ActorMethod<[string, Principal, Role], undefined>,
  'updateSession' : ActorMethod<[string, string, string, string], undefined>,
}
export declare const idlFactory: IDL.InterfaceFactory;
export declare const init: (args: { IDL: typeof IDL }) => IDL.Type[];
