import Cycles "mo:base/ExperimentalCycles";
import Nat "mo:base/Nat";
import AccessControl "authorization/access-control";
import InviteLinksModule "invite-links/invite-links-module";
import Principal "mo:base/Principal";
import OrderedMap "mo:base/OrderedMap";
import Debug "mo:base/Debug";
import Iter "mo:base/Iter";
import Text "mo:base/Text";
import Time "mo:base/Time";
import Random "mo:base/Random";
import Array "mo:base/Array";
import Nat8 "mo:base/Nat8";

actor Main {
  
  let accessControlState = AccessControl.initState();

  
  public shared ({ caller }) func initializeAccessControl() : async () {
    AccessControl.initialize(accessControlState, caller);
  };

  public query ({ caller }) func getCallerUserRole() : async AccessControl.UserRole {
    AccessControl.getUserRole(accessControlState, caller);
  };

  public shared ({ caller }) func assignCallerUserRole(user : Principal, role : AccessControl.UserRole) : async () {
    AccessControl.assignRole(accessControlState, caller, user, role);
  };

  public query ({ caller }) func isCallerAdmin() : async Bool {
    AccessControl.isAdmin(accessControlState, caller);
  };

  public type UserProfile = {
    name : Text;
    
  };

  transient let principalMap = OrderedMap.Make<Principal>(Principal.compare);
  var userProfiles = principalMap.empty<UserProfile>();

  
  public query func isUsernameAvailable(username : Text) : async Bool {
    for ((_, profile) in principalMap.entries(userProfiles)) {
      if (profile.name == username) {
        return false;
      };
    };
    true;
  };

  
  public shared ({ caller }) func saveCallerUserProfile(profile : UserProfile) : async () {
    
    for ((principal, existingProfile) in principalMap.entries(userProfiles)) {
      if (existingProfile.name == profile.name and principal != caller) {
        Debug.trap("Username already exists. Please choose a different username.");
      };
    };

    userProfiles := principalMap.put(userProfiles, caller, profile);
  };

  public query ({ caller }) func getCallerUserProfile() : async ?UserProfile {
    principalMap.get(userProfiles, caller);
  };

  public query func getUserProfile(user : Principal) : async ?UserProfile {
    principalMap.get(userProfiles, user);
  };

  
  public type Campaign = {
    id : Text;
    name : Text;
    description : Text;
    owner : Principal;
    inGameDateTime : Text;
  };

  public type Role = {
    #gm;
    #player;
    #both;
  };

  public type Participant = {
    principal : Principal;
    role : Role;
  };

  public type InvitationStatus = {
    #pending;
    #accepted;
    #rejected;
  };

  public type Invitation = {
    id : Text;
    campaignId : Text;
    sender : Principal;
    recipient : Principal;
    role : Role;
    status : InvitationStatus;
    timestamp : Time.Time;
  };

  public type Session = {
    id : Text;
    campaignId : Text;
    name : Text;
    description : Text;
    date : Text;
    creator : Principal;
  };

  public type SessionPlayer = {
    sessionId : Text;
    participant : Principal;
  };

  public type Note = {
    id : Text;
    sessionId : Text;
    name : Text;
    playerContent : Text;
    gmContent : Text;
    inGameDateTime : ?Text;
    creator : Principal;
    tags : [Text];
  };

  public type NoteAccess = {
    noteId : Text;
    participant : Principal;
    permissions : Nat8;
  };

  public type NoteLink = {
    noteId : Text;
    linkedNoteId : Text;
  };

  public type LinkedNoteDetails = {
    noteId : Text;
    linkedNoteId : Text;
    noteName : Text;
    sessionName : Text;
    tags : [Text];
    hasReadAccess : Bool;
    isOwner : Bool;
  };

  public type MetadataNote = {
    id : Text;
    name : Text;
    content : Text;
    owner : Principal;
    linkedSessionNotes : [Text];
    campaignId : Text;
  };

  transient let textMap = OrderedMap.Make<Text>(Text.compare);
  var campaigns = textMap.empty<Campaign>();
  var invitations = textMap.empty<Invitation>();
  var sessions = textMap.empty<Session>();
  var notes = textMap.empty<Note>();
  var metadataNotes = textMap.empty<MetadataNote>();

  transient let campaignParticipantMap = OrderedMap.Make<Text>(Text.compare);
  var campaignParticipants = campaignParticipantMap.empty<[Participant]>();

  transient let sessionPlayerMap = OrderedMap.Make<Text>(Text.compare);
  var sessionPlayers = sessionPlayerMap.empty<[SessionPlayer]>();

  transient let noteAccessMap = OrderedMap.Make<Text>(Text.compare);
  var noteAccess = noteAccessMap.empty<[NoteAccess]>();

  transient let noteLinkMap = OrderedMap.Make<Text>(Text.compare);
  var noteLinks = noteLinkMap.empty<[NoteLink]>();

  public shared ({ caller }) func createCampaign(id : Text, name : Text, description : Text, inGameDateTime : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can create campaigns");
    };

    let campaign : Campaign = {
      id;
      name;
      description;
      owner = caller;
      inGameDateTime;
    };

    campaigns := textMap.put(campaigns, id, campaign);

    let ownerParticipant : Participant = {
      principal = caller;
      role = #gm;
    };

    campaignParticipants := campaignParticipantMap.put(campaignParticipants, id, [ownerParticipant]);
  };

  public query ({ caller }) func getOwnedCampaigns() : async [Campaign] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view campaigns");
    };

    let userCampaigns = textMap.mapFilter<Campaign, Campaign>(
      campaigns,
      func(_id, campaign) {
        if (campaign.owner == caller) { ?campaign } else { null };
      },
    );

    Iter.toArray(textMap.vals(userCampaigns));
  };

  public query ({ caller }) func getParticipatedCampaigns() : async [Campaign] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view participated campaigns");
    };

    var participatedCampaigns : [Campaign] = [];

    for ((campaignId, participants) in campaignParticipantMap.entries(campaignParticipants)) {
      for (participant in participants.vals()) {
        if (participant.principal == caller) {
          switch (textMap.get(campaigns, campaignId)) {
            case (?campaign) {
              participatedCampaigns := Array.append(participatedCampaigns, [campaign]);
            };
            case (null) {};
          };
        };
      };
    };

    participatedCampaigns;
  };

  public query ({ caller }) func getCampaignDetails(campaignId : Text) : async ?Campaign {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view campaign details");
    };

    textMap.get(campaigns, campaignId);
  };

  public shared ({ caller }) func updateCampaign(id : Text, name : Text, description : Text, inGameDateTime : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can update campaigns");
    };

    switch (textMap.get(campaigns, id)) {
      case (null) { Debug.trap("Campaign not found") };
      case (?campaign) {
        if (campaign.owner != caller) {
          Debug.trap("Unauthorized: Only campaign owners can update their campaigns");
        };

        let updatedCampaign : Campaign = {
          id;
          name;
          description;
          owner = caller;
          inGameDateTime;
        };

        campaigns := textMap.put(campaigns, id, updatedCampaign);
      };
    };
  };

  public shared ({ caller }) func deleteCampaign(id : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can delete campaigns");
    };

    switch (textMap.get(campaigns, id)) {
      case (null) { Debug.trap("Campaign not found") };
      case (?campaign) {
        if (campaign.owner != caller) {
          Debug.trap("Unauthorized: Only campaign owners can delete their campaigns");
        };

        campaigns := textMap.delete(campaigns, id);
        campaignParticipants := campaignParticipantMap.delete(campaignParticipants, id);
      };
    };
  };

  public shared ({ caller }) func addParticipant(campaignId : Text, participant : Principal, role : Role) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can add participants");
    };

    switch (textMap.get(campaigns, campaignId)) {
      case (null) { Debug.trap("Campaign not found") };
      case (?campaign) {
        if (campaign.owner != caller) {
          Debug.trap("Unauthorized: Only campaign owners can add participants");
        };

        let newParticipant : Participant = {
          principal = participant;
          role;
        };

        switch (campaignParticipantMap.get(campaignParticipants, campaignId)) {
          case (null) {
            campaignParticipants := campaignParticipantMap.put(campaignParticipants, campaignId, [newParticipant]);
          };
          case (?existingParticipants) {
            let updatedParticipants = Array.append(existingParticipants, [newParticipant]);
            campaignParticipants := campaignParticipantMap.put(campaignParticipants, campaignId, updatedParticipants);
          };
        };
      };
    };
  };

  public query ({ caller }) func getParticipants(campaignId : Text) : async [Participant] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view participants");
    };

    switch (textMap.get(campaigns, campaignId)) {
      case (null) { Debug.trap("Campaign not found") };
      case (?campaign) {
        switch (campaignParticipantMap.get(campaignParticipants, campaignId)) {
          case (null) { [] };
          case (?participants) { participants };
        };
      };
    };
  };

  public shared ({ caller }) func removeParticipant(campaignId : Text, participant : Principal) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can remove participants");
    };

    switch (textMap.get(campaigns, campaignId)) {
      case (null) { Debug.trap("Campaign not found") };
      case (?campaign) {
        if (campaign.owner != caller) {
          Debug.trap("Unauthorized: Only campaign owners can remove participants");
        };

        switch (campaignParticipantMap.get(campaignParticipants, campaignId)) {
          case (null) { Debug.trap("No participants found for this campaign") };
          case (?participants) {
            let filteredParticipants = Array.filter<Participant>(
              participants,
              func(p) { p.principal != participant },
            );
            campaignParticipants := campaignParticipantMap.put(campaignParticipants, campaignId, filteredParticipants);
          };
        };
      };
    };
  };

  public shared ({ caller }) func updateParticipantRole(campaignId : Text, participant : Principal, newRole : Role) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can update participant roles");
    };

    switch (textMap.get(campaigns, campaignId)) {
      case (null) { Debug.trap("Campaign not found") };
      case (?campaign) {
        if (campaign.owner != caller) {
          Debug.trap("Unauthorized: Only campaign owners can update participant roles");
        };

        switch (campaignParticipantMap.get(campaignParticipants, campaignId)) {
          case (null) { Debug.trap("No participants found for this campaign") };
          case (?participants) {
            let updatedParticipants = Array.map<Participant, Participant>(
              participants,
              func(p) {
                if (p.principal == participant) {
                  { p with role = newRole };
                } else {
                  p;
                };
              },
            );
            campaignParticipants := campaignParticipantMap.put(campaignParticipants, campaignId, updatedParticipants);
          };
        };
      };
    };
  };

  public shared ({ caller }) func sendInvitation(id : Text, campaignId : Text, recipientUsername : Text, role : Role) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can send invitations");
    };

    switch (textMap.get(campaigns, campaignId)) {
      case (null) { Debug.trap("Campaign not found") };
      case (?campaign) {
        if (campaign.owner != caller) {
          Debug.trap("Unauthorized: Only campaign owners can send invitations");
        };

        
        let recipientPrincipal = findPrincipalByUsername(recipientUsername);

        switch (recipientPrincipal) {
          case (null) { Debug.trap("Recipient not found") };
          case (?principal) {
            let invitation : Invitation = {
              id;
              campaignId;
              sender = caller;
              recipient = principal;
              role;
              status = #pending;
              timestamp = Time.now();
            };

            invitations := textMap.put(invitations, id, invitation);
          };
        };
      };
    };
  };

  public query ({ caller }) func getReceivedInvitations() : async [Invitation] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view invitations");
    };

    let userInvitations = textMap.mapFilter<Invitation, Invitation>(
      invitations,
      func(_id, invitation) {
        if (invitation.recipient == caller) { ?invitation } else { null };
      },
    );

    Iter.toArray(textMap.vals(userInvitations));
  };

  public query ({ caller }) func getSentInvitations() : async [Invitation] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view sent invitations");
    };

    let userInvitations = textMap.mapFilter<Invitation, Invitation>(
      invitations,
      func(_id, invitation) {
        if (invitation.sender == caller) { ?invitation } else { null };
      },
    );

    Iter.toArray(textMap.vals(userInvitations));
  };

  public shared ({ caller }) func acceptInvitation(id : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can accept invitations");
    };

    switch (textMap.get(invitations, id)) {
      case (null) { Debug.trap("Invitation not found") };
      case (?invitation) {
        if (invitation.recipient != caller) {
          Debug.trap("Unauthorized: Only the recipient can accept this invitation");
        };

        let updatedInvitation : Invitation = {
          invitation with status = #accepted
        };

        invitations := textMap.put(invitations, id, updatedInvitation);

        let newParticipant : Participant = {
          principal = caller;
          role = invitation.role;
        };

        switch (campaignParticipantMap.get(campaignParticipants, invitation.campaignId)) {
          case (null) {
            campaignParticipants := campaignParticipantMap.put(campaignParticipants, invitation.campaignId, [newParticipant]);
          };
          case (?existingParticipants) {
            let updatedParticipants = Array.append(existingParticipants, [newParticipant]);
            campaignParticipants := campaignParticipantMap.put(campaignParticipants, invitation.campaignId, updatedParticipants);
          };
        };
      };
    };
  };

  public shared ({ caller }) func rejectInvitation(id : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can reject invitations");
    };

    switch (textMap.get(invitations, id)) {
      case (null) { Debug.trap("Invitation not found") };
      case (?invitation) {
        if (invitation.recipient != caller) {
          Debug.trap("Unauthorized: Only the recipient can reject this invitation");
        };

        let updatedInvitation : Invitation = {
          invitation with status = #rejected
        };

        invitations := textMap.put(invitations, id, updatedInvitation);
      };
    };
  };

  
  private func findPrincipalByUsername(username : Text) : ?Principal {
    var foundPrincipal : ?Principal = null;
    for ((principal, profile) in principalMap.entries(userProfiles)) {
      if (profile.name == username) {
        foundPrincipal := ?principal;
      };
    };
    foundPrincipal;
  };

  
  public shared ({ caller }) func leaveCampaign(campaignId : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can leave campaigns");
    };

    switch (textMap.get(campaigns, campaignId)) {
      case (null) { Debug.trap("Campaign not found") };
      case (?campaign) {
        if (campaign.owner == caller) {
          Debug.trap("Campaign owners cannot leave their own campaigns");
        };

        switch (campaignParticipantMap.get(campaignParticipants, campaignId)) {
          case (null) { Debug.trap("No participants found for this campaign") };
          case (?participants) {
            let filteredParticipants = Array.filter<Participant>(
              participants,
              func(p) { p.principal != caller },
            );
            campaignParticipants := campaignParticipantMap.put(campaignParticipants, campaignId, filteredParticipants);
          };
        };
      };
    };
  };

  
  public shared ({ caller }) func createSession(id : Text, campaignId : Text, name : Text, description : Text, date : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can create sessions");
    };

    switch (textMap.get(campaigns, campaignId)) {
      case (null) { Debug.trap("Campaign not found") };
      case (?_campaign) {
        
        switch (campaignParticipantMap.get(campaignParticipants, campaignId)) {
          case (null) { Debug.trap("No participants found for this campaign") };
          case (?participants) {
            let isGM = Array.find<Participant>(
              participants,
              func(p) {
                p.principal == caller and (p.role == #gm or p.role == #both)
              },
            );

            switch (isGM) {
              case (null) { Debug.trap("Unauthorized: Only GMs can create sessions") };
              case (?_gm) {
                let session : Session = {
                  id;
                  campaignId;
                  name;
                  description;
                  date;
                  creator = caller;
                };

                sessions := textMap.put(sessions, id, session);
              };
            };
          };
        };
      };
    };
  };

  public query ({ caller }) func getSessions(campaignId : Text) : async [Session] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view sessions");
    };

    let campaignSessions = textMap.mapFilter<Session, Session>(
      sessions,
      func(_id, session) {
        if (session.campaignId == campaignId) { ?session } else { null };
      },
    );

    Iter.toArray(textMap.vals(campaignSessions));
  };

  
  public shared ({ caller }) func updateSession(id : Text, name : Text, description : Text, date : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can update sessions");
    };

    switch (textMap.get(sessions, id)) {
      case (null) { Debug.trap("Session not found") };
      case (?session) {
        if (session.creator != caller) {
          Debug.trap("Unauthorized: Only session owners can update their sessions");
        };

        let updatedSession : Session = {
          session with
          name;
          description;
          date;
        };

        sessions := textMap.put(sessions, id, updatedSession);
      };
    };
  };

  
  public shared ({ caller }) func deleteSession(id : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can delete sessions");
    };

    switch (textMap.get(sessions, id)) {
      case (null) { Debug.trap("Session not found") };
      case (?session) {
        if (session.creator != caller) {
          Debug.trap("Unauthorized: Only session owners can delete their sessions");
        };

        sessions := textMap.delete(sessions, id);
      };
    };
  };

  
  public shared ({ caller }) func addSessionPlayer(sessionId : Text, participant : Principal) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can add session players");
    };

    switch (textMap.get(sessions, sessionId)) {
      case (null) { Debug.trap("Session not found") };
      case (?session) {
        if (session.creator != caller) {
          Debug.trap("Unauthorized: Only session owners can add session players");
        };

        
        switch (campaignParticipantMap.get(campaignParticipants, session.campaignId)) {
          case (null) { Debug.trap("No participants found for this campaign") };
          case (?participants) {
            let isPlayer = Array.find<Participant>(
              participants,
              func(p) {
                p.principal == participant and (p.role == #player or p.role == #both)
              },
            );

            switch (isPlayer) {
              case (null) { Debug.trap("Participant is not a player in the campaign") };
              case (?_player) {
                let sessionPlayer : SessionPlayer = {
                  sessionId;
                  participant;
                };

                switch (sessionPlayerMap.get(sessionPlayers, sessionId)) {
                  case (null) {
                    sessionPlayers := sessionPlayerMap.put(sessionPlayers, sessionId, [sessionPlayer]);
                  };
                  case (?existingPlayers) {
                    let updatedPlayers = Array.append(existingPlayers, [sessionPlayer]);
                    sessionPlayers := sessionPlayerMap.put(sessionPlayers, sessionId, updatedPlayers);
                  };
                };
              };
            };
          };
        };
      };
    };
  };

  public query ({ caller }) func getSessionPlayers(sessionId : Text) : async [SessionPlayer] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view session players");
    };

    switch (textMap.get(sessions, sessionId)) {
      case (null) { Debug.trap("Session not found") };
      case (?_session) {
        switch (sessionPlayerMap.get(sessionPlayers, sessionId)) {
          case (null) { [] };
          case (?players) { players };
        };
      };
    };
  };

  public shared ({ caller }) func removeSessionPlayer(sessionId : Text, participant : Principal) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can remove session players");
    };

    switch (textMap.get(sessions, sessionId)) {
      case (null) { Debug.trap("Session not found") };
      case (?session) {
        if (session.creator != caller) {
          Debug.trap("Unauthorized: Only session owners can remove session players");
        };

        switch (sessionPlayerMap.get(sessionPlayers, sessionId)) {
          case (null) { Debug.trap("No session players found for this session") };
          case (?players) {
            let filteredPlayers = Array.filter<SessionPlayer>(
              players,
              func(p) { p.participant != participant },
            );
            sessionPlayers := sessionPlayerMap.put(sessionPlayers, sessionId, filteredPlayers);
          };
        };
      };
    };
  };

  
  public shared ({ caller }) func createNote(id : Text, sessionId : Text, name : Text, playerContent : Text, gmContent : Text, inGameDateTime : ?Text, tags : [Text]) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can create notes");
    };

    switch (textMap.get(sessions, sessionId)) {
      case (null) { Debug.trap("Session not found") };
      case (?session) {
        if (session.creator != caller) {
          Debug.trap("Unauthorized: Only session owners can create notes");
        };

        
        if (not isNoteNameUniqueInCampaign(session.campaignId, name)) {
          Debug.trap("Note name must be unique within the campaign");
        };

        let note : Note = {
          id;
          sessionId;
          name;
          playerContent;
          gmContent;
          inGameDateTime;
          creator = caller;
          tags;
        };

        notes := textMap.put(notes, id, note);
      };
    };
  };

  
  private func isNoteNameUniqueInCampaign(campaignId : Text, noteName : Text) : Bool {
    for ((_, note) in textMap.entries(notes)) {
      switch (textMap.get(sessions, note.sessionId)) {
        case (null) {};
        case (?session) {
          if (session.campaignId == campaignId and note.name == noteName) {
            return false;
          };
        };
      };
    };
    true;
  };

  public query ({ caller }) func getNotes(sessionId : Text) : async [Note] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view notes");
    };

    let sessionNotes = textMap.mapFilter<Note, Note>(
      notes,
      func(_id, note) {
        if (note.sessionId == sessionId) { ?note } else { null };
      },
    );

    Iter.toArray(textMap.vals(sessionNotes));
  };

  
  public shared ({ caller }) func manageNoteAccess(noteId : Text, participant : Principal, permissions : Nat8) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can manage note access");
    };

    switch (textMap.get(notes, noteId)) {
      case (null) { Debug.trap("Note not found") };
      case (?note) {
        if (note.creator != caller) {
          Debug.trap("Unauthorized: Only note owners can manage access");
        };

        let access : NoteAccess = {
          noteId;
          participant;
          permissions;
        };

        switch (noteAccessMap.get(noteAccess, noteId)) {
          case (null) {
            noteAccess := noteAccessMap.put(noteAccess, noteId, [access]);
          };
          case (?existingAccess) {
            let updatedAccess = Array.append(existingAccess, [access]);
            noteAccess := noteAccessMap.put(noteAccess, noteId, updatedAccess);
          };
        };
      };
    };
  };

  
  public query ({ caller }) func getNoteAccess(noteId : Text) : async [NoteAccess] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view note access");
    };

    switch (noteAccessMap.get(noteAccess, noteId)) {
      case (null) { [] };
      case (?access) { access };
    };
  };

  
  private func hasWriteAccess(caller : Principal, noteId : Text) : Bool {
    switch (textMap.get(notes, noteId)) {
      case (null) { false };
      case (?note) {
        if (note.creator == caller) {
          return true;
        };
        switch (noteAccessMap.get(noteAccess, noteId)) {
          case (null) { false };
          case (?accessList) {
            let access = Array.find<NoteAccess>(
              accessList,
              func(a) { a.participant == caller },
            );
            switch (access) {
              case (null) { false };
              case (?a) { (a.permissions & 4 : Nat8) != 0 };
            };
          };
        };
      };
    };
  };

  
  public shared ({ caller }) func updateNote(id : Text, name : Text, playerContent : Text, gmContent : Text, inGameDateTime : ?Text, tags : [Text]) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can update notes");
    };

    switch (textMap.get(notes, id)) {
      case (null) { Debug.trap("Note not found") };
      case (?note) {
        if (note.creator != caller and not hasWriteAccess(caller, id)) {
          Debug.trap("Unauthorized: Only note owners or users with write access can update notes");
        };

        
        switch (textMap.get(sessions, note.sessionId)) {
          case (null) { Debug.trap("Session not found") };
          case (?session) {
            if (not isNoteNameUniqueInCampaign(session.campaignId, name)) {
              Debug.trap("Note name must be unique within the campaign");
            };
          };
        };

        let updatedNote : Note = {
          note with
          name;
          playerContent;
          gmContent;
          inGameDateTime;
          tags;
        };

        notes := textMap.put(notes, id, updatedNote);
      };
    };
  };

  
  public shared ({ caller }) func deleteNote(id : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can delete notes");
    };

    switch (textMap.get(notes, id)) {
      case (null) { Debug.trap("Note not found") };
      case (?note) {
        if (note.creator != caller and not hasWriteAccess(caller, id)) {
          Debug.trap("Unauthorized: Only note owners or users with write access can delete notes");
        };

        notes := textMap.delete(notes, id);
      };
    };
  };

  
  public shared ({ caller }) func linkNotes(noteId : Text, targetNoteName : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can link notes");
    };

    switch (textMap.get(notes, noteId)) {
      case (null) { Debug.trap("Note not found") };
      case (?note) {
        if (note.creator != caller and not hasWriteAccess(caller, noteId)) {
          Debug.trap("Unauthorized: Only note owners or users with write access can link notes");
        };

        
        if (note.name == targetNoteName) {
          Debug.trap("Cannot link a note to itself");
        };

        
        let targetNote = findNoteByNameInCampaign(note.sessionId, targetNoteName);

        switch (targetNote) {
          case (null) { Debug.trap("Target note not found in the same campaign") };
          case (?target) {
            
            let link1 : NoteLink = {
              noteId;
              linkedNoteId = target.id;
            };

            let link2 : NoteLink = {
              noteId = target.id;
              linkedNoteId = noteId;
            };

            
            addNoteLink(noteId, link1);
            addNoteLink(target.id, link2);
          };
        };
      };
    };
  };

  
  private func findNoteByNameInCampaign(sessionId : Text, noteName : Text) : ?Note {
    switch (textMap.get(sessions, sessionId)) {
      case (null) { null };
      case (?session) {
        
        let campaignNotes = textMap.mapFilter<Note, Note>(
          notes,
          func(_id, note) {
            switch (textMap.get(sessions, note.sessionId)) {
              case (null) { null };
              case (?noteSession) {
                if (noteSession.campaignId == session.campaignId) {
                  ?note;
                } else {
                  null;
                };
              };
            };
          },
        );

        
        var foundNote : ?Note = null;
        for ((_, note) in textMap.entries(campaignNotes)) {
          if (note.name == noteName) {
            foundNote := ?note;
          };
        };
        foundNote;
      };
    };
  };

  
  private func addNoteLink(noteId : Text, link : NoteLink) {
    switch (noteLinkMap.get(noteLinks, noteId)) {
      case (null) {
        noteLinks := noteLinkMap.put(noteLinks, noteId, [link]);
      };
      case (?existingLinks) {
        let updatedLinks = Array.append(existingLinks, [link]);
        noteLinks := noteLinkMap.put(noteLinks, noteId, updatedLinks);
      };
    };
  };

  
  private func hasReadAccess(participant : Principal, noteId : Text) : Bool {
    switch (textMap.get(notes, noteId)) {
      case (null) { false };
      case (?note) {
        if (note.creator == participant) {
          return true;
        };
        switch (noteAccessMap.get(noteAccess, noteId)) {
          case (null) { false };
          case (?accessList) {
            let access = Array.find<NoteAccess>(
              accessList,
              func(a) { a.participant == participant },
            );
            switch (access) {
              case (null) { false };
              case (?a) {
                
                (a.permissions & 1 : Nat8) != 0 or (a.permissions & 2 : Nat8) != 0;
              };
            };
          };
        };
      };
    };
  };

  
  public query ({ caller }) func getLinkedNotes(noteId : Text) : async [LinkedNoteDetails] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view linked notes");
    };

    switch (noteLinkMap.get(noteLinks, noteId)) {
      case (null) { [] };
      case (?links) {
        let linkedNoteDetails = Array.mapFilter<NoteLink, LinkedNoteDetails>(
          links,
          func(link) {
            switch (textMap.get(notes, link.linkedNoteId)) {
              case (null) { null };
              case (?linkedNote) {
                switch (textMap.get(sessions, linkedNote.sessionId)) {
                  case (null) { null };
                  case (?session) {
                    let hasRead = hasReadAccess(caller, link.linkedNoteId);
                    let isOwner = linkedNote.creator == caller;
                    ?{
                      noteId = link.noteId;
                      linkedNoteId = link.linkedNoteId;
                      noteName = linkedNote.name;
                      sessionName = session.name;
                      tags = linkedNote.tags;
                      hasReadAccess = hasRead;
                      isOwner;
                    };
                  };
                };
              };
            };
          },
        );
        linkedNoteDetails;
      };
    };
  };

  
  public shared ({ caller }) func removeNoteLink(noteId : Text, linkedNoteId : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can remove note links");
    };

    switch (textMap.get(notes, noteId)) {
      case (null) { Debug.trap("Note not found") };
      case (?note) {
        if (note.creator != caller and not hasWriteAccess(caller, noteId)) {
          Debug.trap("Unauthorized: Only note owners or users with write access can remove links");
        };

        
        switch (noteLinkMap.get(noteLinks, noteId)) {
          case (null) {};
          case (?links) {
            let filteredLinks = Array.filter<NoteLink>(
              links,
              func(link) { link.linkedNoteId != linkedNoteId },
            );
            noteLinks := noteLinkMap.put(noteLinks, noteId, filteredLinks);
          };
        };

        
        switch (noteLinkMap.get(noteLinks, linkedNoteId)) {
          case (null) {};
          case (?links) {
            let filteredLinks = Array.filter<NoteLink>(
              links,
              func(link) { link.linkedNoteId != noteId },
            );
            noteLinks := noteLinkMap.put(noteLinks, linkedNoteId, filteredLinks);
          };
        };
      };
    };
  };

  
  public shared ({ caller }) func addTag(noteId : Text, tag : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can add tags");
    };

    switch (textMap.get(notes, noteId)) {
      case (null) { Debug.trap("Note not found") };
      case (?note) {
        if (note.creator != caller and not hasWriteAccess(caller, noteId)) {
          Debug.trap("Unauthorized: Only note owners or users with write access can add tags");
        };

        let updatedTags = Array.append(note.tags, [tag]);
        let updatedNote : Note = {
          note with tags = updatedTags
        };

        notes := textMap.put(notes, noteId, updatedNote);
      };
    };
  };

  public shared ({ caller }) func removeTag(noteId : Text, tag : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can remove tags");
    };

    switch (textMap.get(notes, noteId)) {
      case (null) { Debug.trap("Note not found") };
      case (?note) {
        if (note.creator != caller and not hasWriteAccess(caller, noteId)) {
          Debug.trap("Unauthorized: Only note owners or users with write access can remove tags");
        };

        let filteredTags = Array.filter<Text>(
          note.tags,
          func(t) { t != tag },
        );
        let updatedNote : Note = {
          note with tags = filteredTags
        };

        notes := textMap.put(notes, noteId, updatedNote);
      };
    };
  };

  
  public query ({ caller }) func getUserCampaignTags() : async [Text] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view tags");
    };

    var userCampaignIds : [Text] = [];

    
    for ((campaignId, participants) in campaignParticipantMap.entries(campaignParticipants)) {
      for (participant in participants.vals()) {
        if (participant.principal == caller) {
          userCampaignIds := Array.append(userCampaignIds, [campaignId]);
        };
      };
    };

    var uniqueTags : [Text] = [];

    
    for ((_, note) in textMap.entries(notes)) {
      switch (textMap.get(sessions, note.sessionId)) {
        case (null) {};
        case (?session) {
          if (Array.find<Text>(userCampaignIds, func(id) { id == session.campaignId }) != null) {
            for (tag in note.tags.vals()) {
              if (Array.find<Text>(uniqueTags, func(t) { t == tag }) == null) {
                uniqueTags := Array.append(uniqueTags, [tag]);
              };
            };
          };
        };
      };
    };

    uniqueTags;
  };

  
  public query ({ caller }) func getNotesByTag(tag : Text) : async [Note] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view notes by tag");
    };

    var userCampaignIds : [Text] = [];

    
    for ((campaignId, participants) in campaignParticipantMap.entries(campaignParticipants)) {
      for (participant in participants.vals()) {
        if (participant.principal == caller) {
          userCampaignIds := Array.append(userCampaignIds, [campaignId]);
        };
      };
    };

    var notesWithTag : [Note] = [];

    
    for ((_, note) in textMap.entries(notes)) {
      switch (textMap.get(sessions, note.sessionId)) {
        case (null) {};
        case (?session) {
          if (Array.find<Text>(userCampaignIds, func(id) { id == session.campaignId }) != null) {
            if (Array.find<Text>(note.tags, func(t) { t == tag }) != null) {
              notesWithTag := Array.append(notesWithTag, [note]);
            };
          };
        };
      };
    };

    notesWithTag;
  };

  
  public shared ({ caller }) func createMetadataNote(id : Text, name : Text, content : Text, linkedSessionNotes : [Text], campaignId : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can create metadata notes");
    };

    
    switch (textMap.get(campaigns, campaignId)) {
      case (null) { Debug.trap("Campaign not found") };
      case (?campaign) {
        if (campaign.owner != caller and not isParticipantInCampaign(caller, campaignId)) {
          Debug.trap("Unauthorized: Only campaign owners or participants can create metadata notes");
        };

        
        if (not isMetadataNoteNameUniqueInCampaign(campaignId, name)) {
          Debug.trap("Metadata note name must be unique within the campaign");
        };

        let metadataNote : MetadataNote = {
          id;
          name;
          content;
          owner = caller;
          linkedSessionNotes;
          campaignId;
        };

        metadataNotes := textMap.put(metadataNotes, id, metadataNote);
      };
    };
  };

  
  private func isMetadataNoteNameUniqueInCampaign(campaignId : Text, metadataNoteName : Text) : Bool {
    for ((_, metadataNote) in textMap.entries(metadataNotes)) {
      if (metadataNote.campaignId == campaignId and metadataNote.name == metadataNoteName) {
        return false;
      };
    };
    true;
  };

  
  private func isParticipantInCampaign(caller : Principal, campaignId : Text) : Bool {
    switch (campaignParticipantMap.get(campaignParticipants, campaignId)) {
      case (null) { false };
      case (?participants) {
        for (participant in participants.vals()) {
          if (participant.principal == caller) {
            return true;
          };
        };
        false;
      };
    };
  };

  
  public query ({ caller }) func getMetadataNotes() : async [MetadataNote] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view metadata notes");
    };

    let allMetadataNotes = Iter.toArray(textMap.vals(metadataNotes));
    let accessibleMetadataNotes = Array.filter<MetadataNote>(
      allMetadataNotes,
      func(metadataNote) {
        metadataNote.owner == caller or isParticipantInCampaign(caller, metadataNote.campaignId);
      },
    );

    accessibleMetadataNotes;
  };

  
  public query ({ caller }) func getLinkedMetadataNotes(sessionNoteId : Text) : async [MetadataNote] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view linked metadata notes");
    };

    let linkedMetadataNotes = Array.filter<MetadataNote>(
      Iter.toArray(textMap.vals(metadataNotes)),
      func(metadataNote) {
        Array.find<Text>(metadataNote.linkedSessionNotes, func(id) { id == sessionNoteId }) != null;
      },
    );

    linkedMetadataNotes;
  };

  
  public shared ({ caller }) func unlinkMetadataNote(metadataNoteId : Text, sessionNoteId : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can unlink metadata notes");
    };

    switch (textMap.get(metadataNotes, metadataNoteId)) {
      case (null) { Debug.trap("Metadata note not found") };
      case (?metadataNote) {
        if (metadataNote.owner != caller) {
          Debug.trap("Unauthorized: Only metadata note owners can unlink metadata notes");
        };

        let updatedLinkedSessionNotes = Array.filter<Text>(
          metadataNote.linkedSessionNotes,
          func(id) { id != sessionNoteId },
        );

        let updatedMetadataNote : MetadataNote = {
          metadataNote with linkedSessionNotes = updatedLinkedSessionNotes
        };

        metadataNotes := textMap.put(metadataNotes, metadataNoteId, updatedMetadataNote);
      };
    };
  };

  
  public query ({ caller }) func getMetadataNoteDetails(metadataNoteId : Text) : async ?MetadataNote {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can view metadata note details");
    };

    switch (textMap.get(metadataNotes, metadataNoteId)) {
      case (null) { null };
      case (?metadataNote) {
        if (metadataNote.owner == caller or isParticipantInCampaign(caller, metadataNote.campaignId)) {
          ?metadataNote;
        } else {
          Debug.trap("Unauthorized: You do not have access to this metadata note");
        };
      };
    };
  };

  
  public shared ({ caller }) func linkMetadataNoteByName(sessionNoteId : Text, metadataNoteName : Text, campaignId : Text) : async () {
    if (not (AccessControl.hasPermission(accessControlState, caller, #user))) {
      Debug.trap("Unauthorized: Only users can link metadata notes");
    };

    
    switch (textMap.get(notes, sessionNoteId)) {
      case (null) { Debug.trap("Session note not found") };
      case (?_sessionNote) {
        
        let metadataNote = findMetadataNoteByNameAndCampaign(metadataNoteName, campaignId);

        switch (metadataNote) {
          case (null) { Debug.trap("Metadata note not found in the same campaign") };
          case (?note) {
            
            let updatedLinkedSessionNotes = Array.append(note.linkedSessionNotes, [sessionNoteId]);
            let updatedMetadataNote : MetadataNote = {
              note with linkedSessionNotes = updatedLinkedSessionNotes
            };

            metadataNotes := textMap.put(metadataNotes, note.id, updatedMetadataNote);
          };
        };
      };
    };
  };

  
  private func findMetadataNoteByNameAndCampaign(metadataNoteName : Text, campaignId : Text) : ?MetadataNote {
    for ((_, metadataNote) in textMap.entries(metadataNotes)) {
      if (metadataNote.name == metadataNoteName and metadataNote.campaignId == campaignId) {
        return ?metadataNote;
      };
    };
    null;
  };

  
  let inviteState = InviteLinksModule.initState();

  public shared ({ caller }) func generateInviteCode() : async Text {
    if (not (AccessControl.hasPermission(accessControlState, caller, #admin))) {
      Debug.trap("Unauthorized: Only admins can generate invite codes");
    };
    let blob = await Random.blob();
    let code = InviteLinksModule.generateUUID(blob);
    InviteLinksModule.generateInviteCode(inviteState, code);
    code;
  };

  public shared func submitRSVP(name : Text, attending : Bool, inviteCode : Text) : async () {
    InviteLinksModule.submitRSVP(inviteState, name, attending, inviteCode);
  };

  public query ({ caller }) func getAllRSVPs() : async [InviteLinksModule.RSVP] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #admin))) {
      Debug.trap("Unauthorized: Only admins can view RSVPs");
    };
    InviteLinksModule.getAllRSVPs(inviteState);
  };

  public query ({ caller }) func getInviteCodes() : async [InviteLinksModule.InviteCode] {
    if (not (AccessControl.hasPermission(accessControlState, caller, #admin))) {
      Debug.trap("Unauthorized: Only admins can view invite codes");
    };
    InviteLinksModule.getInviteCodes(inviteState);
  };

type __CAFFEINE_STORAGE_RefillInformation = {
    proposed_top_up_amount: ?Nat;
};

type __CAFFEINE_STORAGE_RefillResult = {
    success: ?Bool;
    topped_up_amount: ?Nat;
};

    public shared (msg) func __CAFFEINE_STORAGE_refillCashier(refill_information: ?__CAFFEINE_STORAGE_RefillInformation) : async __CAFFEINE_STORAGE_RefillResult {
    let cashier = Principal.fromText("72ch2-fiaaa-aaaar-qbsvq-cai");
    
    assert (cashier == msg.caller);
    
    let current_balance = Cycles.balance();
    let reserved_cycles : Nat = 400_000_000_000;
    
    let current_free_cycles_count : Nat = Nat.sub(current_balance, reserved_cycles);
    
    let cycles_to_send : Nat = switch (refill_information) {
        case null { current_free_cycles_count };
        case (?info) {
            switch (info.proposed_top_up_amount) {
                case null { current_free_cycles_count };
                case (?proposed) { Nat.min(proposed, current_free_cycles_count) };
            }
        };
    };

    let target_canister = actor(Principal.toText(cashier)) : actor {
        account_top_up_v1 : ({ account : Principal }) -> async ();
    };
    
    let current_principal = Principal.fromActor(Main);
    
    await (with cycles = cycles_to_send) target_canister.account_top_up_v1({ account = current_principal });
    
    return {
        success = ?true;
        topped_up_amount = ?cycles_to_send;
    };
};
};

