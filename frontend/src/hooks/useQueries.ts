import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useActor } from './useActor';
import { Campaign, UserProfile, Participant, Invitation, Role, Session, SessionPlayer, Note, NoteAccess, LinkedNoteDetails, MetadataNote } from '../backend';
import { toast } from 'sonner';
import { Principal } from '@dfinity/principal';

export function useGetCallerUserProfile() {
  const { actor, isFetching: actorFetching } = useActor();

  const query = useQuery<UserProfile | null>({
    queryKey: ['currentUserProfile'],
    queryFn: async () => {
      if (!actor) throw new Error('Actor not available');
      return actor.getCallerUserProfile();
    },
    enabled: !!actor && !actorFetching,
    retry: false,
  });

  return {
    ...query,
    isLoading: actorFetching || query.isLoading,
    isFetched: !!actor && query.isFetched,
  };
}

export function useIsUsernameAvailable() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (username: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.isUsernameAvailable(username);
    },
  });
}

export function useSaveCallerUserProfile() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (profile: UserProfile) => {
      if (!actor) throw new Error('Actor not available');
      return actor.saveCallerUserProfile(profile);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['currentUserProfile'] });
      toast.success('Profile saved successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to save profile: ${error.message}`);
    },
  });
}

export function useGetUserProfile() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (user: Principal) => {
      if (!actor) throw new Error('Actor not available');
      return actor.getUserProfile(user);
    },
  });
}

export function useGetOwnedCampaigns() {
  const { actor, isFetching: actorFetching } = useActor();

  return useQuery<Campaign[]>({
    queryKey: ['ownedCampaigns'],
    queryFn: async () => {
      if (!actor) return [];
      return actor.getOwnedCampaigns();
    },
    enabled: !!actor && !actorFetching,
  });
}

export function useGetParticipatedCampaigns() {
  const { actor, isFetching: actorFetching } = useActor();

  return useQuery<Campaign[]>({
    queryKey: ['participatedCampaigns'],
    queryFn: async () => {
      if (!actor) return [];
      return actor.getParticipatedCampaigns();
    },
    enabled: !!actor && !actorFetching,
  });
}

export function useGetCampaignDetails() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (campaignId: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.getCampaignDetails(campaignId);
    },
  });
}

// Keep the old method for backward compatibility
export function useGetCampaigns() {
  return useGetOwnedCampaigns();
}

export function useCreateCampaign() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, name, description, inGameDateTime }: { id: string; name: string; description: string; inGameDateTime: string }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.createCampaign(id, name, description, inGameDateTime);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ownedCampaigns'] });
      queryClient.invalidateQueries({ queryKey: ['participatedCampaigns'] });
      toast.success('Campaign created successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to create campaign: ${error.message}`);
    },
  });
}

export function useUpdateCampaign() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, name, description, inGameDateTime }: { id: string; name: string; description: string; inGameDateTime: string }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.updateCampaign(id, name, description, inGameDateTime);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ownedCampaigns'] });
      queryClient.invalidateQueries({ queryKey: ['participatedCampaigns'] });
      toast.success('Campaign updated successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to update campaign: ${error.message}`);
    },
  });
}

export function useDeleteCampaign() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.deleteCampaign(id);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ownedCampaigns'] });
      queryClient.invalidateQueries({ queryKey: ['participatedCampaigns'] });
      toast.success('Campaign deleted successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to delete campaign: ${error.message}`);
    },
  });
}

export function useGetParticipants() {
  const { actor, isFetching: actorFetching } = useActor();

  return useMutation({
    mutationFn: async (campaignId: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.getParticipants(campaignId);
    },
  });
}

export function useAddParticipant() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ campaignId, participant, role }: { campaignId: string; participant: Principal; role: Role }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.addParticipant(campaignId, participant, role);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['participants'] });
      toast.success('Participant added successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to add participant: ${error.message}`);
    },
  });
}

export function useRemoveParticipant() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ campaignId, participant }: { campaignId: string; participant: Principal }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.removeParticipant(campaignId, participant);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['participants'] });
      toast.success('Participant removed successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to remove participant: ${error.message}`);
    },
  });
}

export function useUpdateParticipantRole() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ campaignId, participant, newRole }: { campaignId: string; participant: Principal; newRole: Role }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.updateParticipantRole(campaignId, participant, newRole);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['participants'] });
      toast.success('Participant role updated successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to update participant role: ${error.message}`);
    },
  });
}

export function useLeaveCampaign() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (campaignId: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.leaveCampaign(campaignId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ownedCampaigns'] });
      queryClient.invalidateQueries({ queryKey: ['participatedCampaigns'] });
      queryClient.invalidateQueries({ queryKey: ['participants'] });
      toast.success('You have left the campaign successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to leave campaign: ${error.message}`);
    },
  });
}

export function useSendInvitation() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, campaignId, recipientUsername, role }: { id: string; campaignId: string; recipientUsername: string; role: Role }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.sendInvitation(id, campaignId, recipientUsername, role);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sentInvitations'] });
      toast.success('Invitation sent successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to send invitation: ${error.message}`);
    },
  });
}

export function useGetReceivedInvitations() {
  const { actor, isFetching: actorFetching } = useActor();

  return useQuery<Invitation[]>({
    queryKey: ['receivedInvitations'],
    queryFn: async () => {
      if (!actor) return [];
      return actor.getReceivedInvitations();
    },
    enabled: !!actor && !actorFetching,
  });
}

export function useGetSentInvitations() {
  const { actor, isFetching: actorFetching } = useActor();

  return useQuery<Invitation[]>({
    queryKey: ['sentInvitations'],
    queryFn: async () => {
      if (!actor) return [];
      return actor.getSentInvitations();
    },
    enabled: !!actor && !actorFetching,
  });
}

export function useAcceptInvitation() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.acceptInvitation(id);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['receivedInvitations'] });
      queryClient.invalidateQueries({ queryKey: ['ownedCampaigns'] });
      queryClient.invalidateQueries({ queryKey: ['participatedCampaigns'] });
      toast.success('Invitation accepted successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to accept invitation: ${error.message}`);
    },
  });
}

export function useRejectInvitation() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.rejectInvitation(id);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['receivedInvitations'] });
      toast.success('Invitation rejected successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to reject invitation: ${error.message}`);
    },
  });
}

// Session Management Hooks
export function useCreateSession() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, campaignId, name, description, date }: { id: string; campaignId: string; name: string; description: string; date: string }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.createSession(id, campaignId, name, description, date);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['sessions', variables.campaignId] });
      toast.success('Session created successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to create session: ${error.message}`);
    },
  });
}

export function useGetSessions() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (campaignId: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.getSessions(campaignId);
    },
  });
}

export function useUpdateSession() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, name, description, date }: { id: string; name: string; description: string; date: string }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.updateSession(id, name, description, date);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sessions'] });
      toast.success('Session updated successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to update session: ${error.message}`);
    },
  });
}

export function useDeleteSession() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.deleteSession(id);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sessions'] });
      toast.success('Session deleted successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to delete session: ${error.message}`);
    },
  });
}

// Session Player Management Hooks
export function useGetSessionPlayers() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (sessionId: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.getSessionPlayers(sessionId);
    },
  });
}

export function useAddSessionPlayer() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ sessionId, participant }: { sessionId: string; participant: Principal }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.addSessionPlayer(sessionId, participant);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['sessionPlayers', variables.sessionId] });
      toast.success('Session player added successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to add session player: ${error.message}`);
    },
  });
}

export function useRemoveSessionPlayer() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ sessionId, participant }: { sessionId: string; participant: Principal }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.removeSessionPlayer(sessionId, participant);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['sessionPlayers', variables.sessionId] });
      toast.success('Session player removed successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to remove session player: ${error.message}`);
    },
  });
}

// Note Management Hooks
export function useCreateNote() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, sessionId, name, playerContent, gmContent, inGameDateTime, tags }: { id: string; sessionId: string; name: string; playerContent: string; gmContent: string; inGameDateTime?: string; tags?: string[] }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.createNote(id, sessionId, name, playerContent, gmContent, inGameDateTime || null, tags || []);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['notes', variables.sessionId] });
      queryClient.invalidateQueries({ queryKey: ['userCampaignTags'] });
      toast.success('Note created successfully!');
    },
    onError: (error: Error) => {
      const errorMessage = error.message.toLowerCase();
      if (errorMessage.includes('unique') && errorMessage.includes('campaign')) {
        toast.error('A note with this name already exists in this campaign. Please choose a different name.');
      } else {
        toast.error(`Failed to create note: ${error.message}`);
      }
    },
  });
}

export function useGetNotes() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (sessionId: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.getNotes(sessionId);
    },
  });
}

export function useUpdateNote() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, name, playerContent, gmContent, inGameDateTime, tags }: { id: string; name: string; playerContent: string; gmContent: string; inGameDateTime?: string; tags?: string[] }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.updateNote(id, name, playerContent, gmContent, inGameDateTime || null, tags || []);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notes'] });
      queryClient.invalidateQueries({ queryKey: ['linkedNotes'] });
      queryClient.invalidateQueries({ queryKey: ['userCampaignTags'] });
      toast.success('Note updated successfully!');
    },
    onError: (error: Error) => {
      const errorMessage = error.message.toLowerCase();
      if (errorMessage.includes('unique') && errorMessage.includes('campaign')) {
        toast.error('A note with this name already exists in this campaign. Please choose a different name or keep the current name.');
      } else {
        toast.error(`Failed to update note: ${error.message}`);
      }
    },
  });
}

export function useDeleteNote() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.deleteNote(id);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notes'] });
      queryClient.invalidateQueries({ queryKey: ['linkedNotes'] });
      queryClient.invalidateQueries({ queryKey: ['userCampaignTags'] });
      toast.success('Note deleted successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to delete note: ${error.message}`);
    },
  });
}

// Note Access Management Hooks
export function useManageNoteAccess() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ noteId, participant, permissions }: { noteId: string; participant: Principal; permissions: bigint }) => {
      if (!actor) throw new Error('Actor not available');
      // Convert bigint to number for backend compatibility
      return actor.manageNoteAccess(noteId, participant, Number(permissions));
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['noteAccess', variables.noteId] });
      toast.success('Note access updated successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to update note access: ${error.message}`);
    },
  });
}

export function useGetNoteAccess() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (noteId: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.getNoteAccess(noteId);
    },
  });
}

// Note Linking Hooks
export function useLinkNotes() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ noteId, targetNoteName }: { noteId: string; targetNoteName: string }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.linkNotes(noteId, targetNoteName);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['linkedNotes', variables.noteId] });
      toast.success('Notes linked successfully!');
    },
    onError: (error: Error) => {
      // Check for specific error messages and provide user-friendly feedback
      const errorMessage = error.message.toLowerCase();
      if (errorMessage.includes('already exists') || errorMessage.includes('duplicate')) {
        toast.error('A link already exists between these notes. Please choose a different note.');
      } else if (errorMessage.includes('cannot link') && errorMessage.includes('itself')) {
        toast.error('Cannot link a note to itself. Please choose a different note.');
      } else {
        toast.error(`Failed to link notes: ${error.message}`);
      }
    },
  });
}

export function useGetLinkedNotes() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (noteId: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.getLinkedNotes(noteId);
    },
  });
}

export function useRemoveNoteLink() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ noteId, linkedNoteId }: { noteId: string; linkedNoteId: string }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.removeNoteLink(noteId, linkedNoteId);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['linkedNotes', variables.noteId] });
      queryClient.invalidateQueries({ queryKey: ['linkedNotes', variables.linkedNoteId] });
      toast.success('Note link removed successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to remove note link: ${error.message}`);
    },
  });
}

// Note Tag Management Hooks
export function useAddTag() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ noteId, tag }: { noteId: string; tag: string }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.addTag(noteId, tag);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notes'] });
      queryClient.invalidateQueries({ queryKey: ['userCampaignTags'] });
      toast.success('Tag added successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to add tag: ${error.message}`);
    },
  });
}

export function useRemoveTag() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ noteId, tag }: { noteId: string; tag: string }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.removeTag(noteId, tag);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notes'] });
      queryClient.invalidateQueries({ queryKey: ['userCampaignTags'] });
      toast.success('Tag removed successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to remove tag: ${error.message}`);
    },
  });
}

// Tag Navigation Hooks
export function useGetUserCampaignTags() {
  const { actor, isFetching: actorFetching } = useActor();

  return useQuery<string[]>({
    queryKey: ['userCampaignTags'],
    queryFn: async () => {
      if (!actor) return [];
      return actor.getUserCampaignTags();
    },
    enabled: !!actor && !actorFetching,
  });
}

export function useGetNotesByTag() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (tag: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.getNotesByTag(tag);
    },
  });
}

// Cross-session note finding hook for navigation
export function useFindNoteAcrossSessions() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async ({ campaignId, noteId }: { campaignId: string; noteId: string }) => {
      if (!actor) throw new Error('Actor not available');
      
      // Get all sessions in the campaign
      const sessions = await actor.getSessions(campaignId);
      
      // Search through each session to find the note
      for (const session of sessions) {
        try {
          const notes = await actor.getNotes(session.id);
          const targetNote = notes.find(note => note.id === noteId);
          if (targetNote) {
            return {
              note: targetNote,
              session: session
            };
          }
        } catch (error) {
          // Continue searching if we can't access this session's notes
          continue;
        }
      }
      
      return null;
    },
  });
}

// Metadata Note Management Hooks
export function useCreateMetadataNote() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, name, content, linkedSessionNotes, campaignId }: { id: string; name: string; content: string; linkedSessionNotes: string[]; campaignId: string }) => {
      if (!actor) throw new Error('Actor not available');
      
      // Validate JSON format
      try {
        JSON.parse(content);
      } catch (error) {
        throw new Error('Invalid JSON format. Please check your JSON syntax.');
      }
      
      // Backend expects campaignId as the last parameter
      return actor.createMetadataNote(id, name, content, linkedSessionNotes, campaignId);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['metadataNotes'] });
      queryClient.invalidateQueries({ queryKey: ['campaignMetadataNotes', variables.campaignId] });
      queryClient.invalidateQueries({ queryKey: ['linkedMetadataNotes'] });
      toast.success('Metadata note created successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to create metadata note: ${error.message}`);
    },
  });
}

export function useGetMetadataNotes() {
  const { actor, isFetching: actorFetching } = useActor();

  return useQuery<MetadataNote[]>({
    queryKey: ['metadataNotes'],
    queryFn: async () => {
      if (!actor) return [];
      return actor.getMetadataNotes();
    },
    enabled: !!actor && !actorFetching,
    staleTime: 0, // Always refetch to ensure fresh data
    refetchOnMount: true, // Always refetch when component mounts
  });
}

export function useGetLinkedMetadataNotes() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (sessionNoteId: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.getLinkedMetadataNotes(sessionNoteId);
    },
  });
}

export function useUnlinkMetadataNote() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ metadataNoteId, sessionNoteId }: { metadataNoteId: string; sessionNoteId: string }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.unlinkMetadataNote(metadataNoteId, sessionNoteId);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['linkedMetadataNotes', variables.sessionNoteId] });
      queryClient.invalidateQueries({ queryKey: ['metadataNotes'] });
      queryClient.invalidateQueries({ queryKey: ['campaignMetadataNotes'] });
      toast.success('Metadata note unlinked successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to unlink metadata note: ${error.message}`);
    },
  });
}

export function useGetMetadataNoteDetails() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (metadataNoteId: string) => {
      if (!actor) throw new Error('Actor not available');
      return actor.getMetadataNoteDetails(metadataNoteId);
    },
  });
}

// Helper function to check if user has access to view metadata note details
export function useCheckMetadataNoteAccess() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (metadataNoteId: string) => {
      if (!actor) throw new Error('Actor not available');
      
      try {
        // Try to get metadata note details - this will fail if user doesn't have access
        const metadataNote = await actor.getMetadataNoteDetails(metadataNoteId);
        return metadataNote !== null;
      } catch (error) {
        // If the call fails, user doesn't have access
        return false;
      }
    },
  });
}

// Get metadata notes for a specific campaign with React Query - filters client-side since backend doesn't support campaign filtering
export function useGetCampaignMetadataNotes(campaignId: string) {
  const { actor, isFetching: actorFetching } = useActor();

  return useQuery<MetadataNote[]>({
    queryKey: ['campaignMetadataNotes', campaignId],
    queryFn: async () => {
      if (!actor) return [];
      const allMetadataNotes = await actor.getMetadataNotes();
      // Filter client-side to get only metadata notes for this campaign
      return allMetadataNotes.filter(note => note.campaignId === campaignId);
    },
    enabled: !!actor && !actorFetching && !!campaignId,
    staleTime: 30000, // Cache for 30 seconds
  });
}

// Legacy mutation version for backward compatibility
export function useGetCampaignMetadataNotesLegacy() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (campaignId: string) => {
      if (!actor) throw new Error('Actor not available');
      const allMetadataNotes = await actor.getMetadataNotes();
      // Filter client-side to get only metadata notes for this campaign
      return allMetadataNotes.filter(note => note.campaignId === campaignId);
    },
  });
}

// New hook to link metadata notes by name with campaign validation
export function useLinkMetadataNoteByName() {
  const { actor } = useActor();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ sessionNoteId, metadataNoteName, campaignId }: { sessionNoteId: string; metadataNoteName: string; campaignId: string }) => {
      if (!actor) throw new Error('Actor not available');
      return actor.linkMetadataNoteByName(sessionNoteId, metadataNoteName, campaignId);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['linkedMetadataNotes', variables.sessionNoteId] });
      queryClient.invalidateQueries({ queryKey: ['metadataNotes'] });
      queryClient.invalidateQueries({ queryKey: ['campaignMetadataNotes'] });
      toast.success('Metadata note linked successfully!');
    },
    onError: (error: Error) => {
      toast.error(`Failed to link metadata note: ${error.message}`);
    },
  });
}

// Helper function to check if user has write access to any session note in a campaign
export function useCheckWriteAccessInCampaign() {
  const { actor } = useActor();

  return useMutation({
    mutationFn: async (campaignId: string) => {
      if (!actor) throw new Error('Actor not available');
      
      try {
        // Get all sessions in the campaign
        const sessions = await actor.getSessions(campaignId);
        
        // Check each session for notes where user has write access
        for (const session of sessions) {
          try {
            const notes = await actor.getNotes(session.id);
            
            // Check if user is owner of any note or has write access
            for (const note of notes) {
              // If user is note creator, they have write access
              const userPrincipal = (await actor.getCallerUserProfile())?.name; // This is a workaround
              if (note.creator.toString() === userPrincipal) {
                return true;
              }
              
              // Check explicit write permissions
              try {
                const accessList = await actor.getNoteAccess(note.id);
                const userAccess = accessList.find(access => 
                  access.participant.toString() === userPrincipal
                );
                if (userAccess && (userAccess.permissions & 4) !== 0) { // Write permission bit
                  return true;
                }
              } catch {
                // Continue checking other notes
                continue;
              }
            }
          } catch {
            // Continue checking other sessions
            continue;
          }
        }
        
        return false;
      } catch (error) {
        console.error('Failed to check write access:', error);
        return false;
      }
    },
  });
}
